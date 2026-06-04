using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace SistemaFlota
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class SolicitudTallerController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly AuditoriaService _auditoria;
        private readonly ITwilioService _twilio;

        public SolicitudTallerController(
            AppDbContext context,
            AuditoriaService auditoria,
            ITwilioService twilio)
        {
            _context = context;
            _auditoria = auditoria;
            _twilio = twilio;
        }

        private string GetUsuario() =>
            User.FindFirst(ClaimTypes.Name)?.Value ?? "Desconocido";
        private string GetRol() =>
            User.FindFirst(ClaimTypes.Role)?.Value ?? "Desconocido";

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var lista = await _context.SolicitudesTaller
                .Include(s => s.Conductor)
                .Include(s => s.Vehiculo)
                .OrderByDescending(s => s.FechaSolicitud)
                .Select(s => new
                {
                    s.Id,
                    s.FechaSolicitud,
                    s.TipoMantenimiento,
                    s.DescripcionProblema,
                    s.FotoOdometro,
                    s.Kilometraje,
                    s.Estado,
                    s.AutorizadoPor,
                    s.ObservacionAut,
                    s.FechaAutorizacion,
                    s.NumeroFacturaTaller,
                    s.ValorFactura,
                    s.FechaFactura,
                    s.FacturaValidada,
                    s.ObservacionFactura,
                    Conductor = new { s.Conductor!.Id, s.Conductor.Nombre, s.Conductor.Telefono },
                    Vehiculo = new { s.Vehiculo!.Id, s.Vehiculo.Placa, s.Vehiculo.Marca, s.Vehiculo.Modelo }
                })
                .ToListAsync();
            return Ok(lista);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var s = await _context.SolicitudesTaller
                .Include(s => s.Conductor)
                .Include(s => s.Vehiculo)
                .FirstOrDefaultAsync(s => s.Id == id);
            if (s == null) return NotFound();
            return Ok(s);
        }

        [HttpPost]
        public async Task<IActionResult> Post(
            [FromForm] int ConductorId,
            [FromForm] int VehiculoId,
            [FromForm] string TipoMantenimiento,
            [FromForm] string DescripcionProblema,
            [FromForm] int? Kilometraje,
            IFormFile? FotoOdometro)
        {
            try
            {
                string? nombreFoto = null;
                if (FotoOdometro != null)
                {
                    var carpeta = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/solicitudes-taller");
                    if (!Directory.Exists(carpeta)) Directory.CreateDirectory(carpeta);
                    nombreFoto = Guid.NewGuid().ToString() + Path.GetExtension(FotoOdometro.FileName);
                    using var stream = new FileStream(Path.Combine(carpeta, nombreFoto), FileMode.Create);
                    await FotoOdometro.CopyToAsync(stream);
                }

                var solicitud = new SolicitudTaller
                {
                    ConductorId = ConductorId,
                    VehiculoId = VehiculoId,
                    TipoMantenimiento = TipoMantenimiento,
                    DescripcionProblema = DescripcionProblema,
                    Kilometraje = Kilometraje,
                    FotoOdometro = nombreFoto,
                    Estado = "Pendiente",
                    FechaSolicitud = DateTime.Now
                };

                _context.SolicitudesTaller.Add(solicitud);
                await _context.SaveChangesAsync();

                var conductor = await _context.Conductores.FindAsync(ConductorId);
                var vehiculo = await _context.Vehiculos.FindAsync(VehiculoId);

                await _auditoria.RegistrarAsync(
                    usuario: GetUsuario(), rol: GetRol(),
                    accion: "Crear", modulo: "SolicitudTaller",
                    detalle: $"Solicitud taller — Conductor: {conductor?.Nombre ?? "-"}, Vehículo: {vehiculo?.Placa ?? "-"}, Tipo: {TipoMantenimiento}",
                    registroId: solicitud.Id
                );

                // ── TWILIO ──
                var hora = DateTime.Now.ToString("hh:mm tt");
                var fecha = DateTime.Now.ToString("dd/MM/yyyy");
                var mensajeGrupo =
                    $"🔧 *SOLICITUD TALLER*\n" +
                    $"👤 Conductor: {conductor?.Nombre ?? "-"}\n" +
                    $"🚗 Vehículo: {vehiculo?.Placa ?? "-"} {vehiculo?.Marca ?? ""} {vehiculo?.Modelo ?? ""}\n" +
                    $"⚙️ Tipo: {TipoMantenimiento}\n" +
                    $"📋 Descripción: {DescripcionProblema}\n" +
                    $"🛣 Km: {Kilometraje?.ToString() ?? "No registrado"}\n" +
                    $"🕐 Hora: {hora} — {fecha}\n" +
                    $"⚠️ Requiere autorización";

                var numerosGrupo = await _context.ContactosNotificacion
                    .Where(c => c.Activo && c.RecibeIncidentes)
                    .Select(c => c.NumeroWhatsApp)
                    .ToListAsync();

                Console.WriteLine($"📱 Contactos grupo: {numerosGrupo.Count}");
                if (numerosGrupo.Any())
                    await _twilio.EnviarAMultiplesAsync(numerosGrupo, mensajeGrupo);

                return Ok(solicitud);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.InnerException?.Message ?? ex.Message);
            }
        }

        [HttpPut("{id}/autorizar")]
        public async Task<IActionResult> Autorizar(int id, [FromBody] AutorizarSolicitudDto dto)
        {
            var s = await _context.SolicitudesTaller
                .Include(s => s.Conductor)
                .Include(s => s.Vehiculo)
                .FirstOrDefaultAsync(s => s.Id == id);
            if (s == null) return NotFound();

            s.Estado = "Autorizado";
            s.AutorizadoPor = dto.AutorizadoPor;
            s.ObservacionAut = dto.Observacion;
            s.FechaAutorizacion = DateTime.Now;
            await _context.SaveChangesAsync();

            await _auditoria.RegistrarAsync(
                usuario: GetUsuario(), rol: GetRol(),
                accion: "Autorizar", modulo: "SolicitudTaller",
                detalle: $"Solicitud #{id} AUTORIZADA por: {dto.AutorizadoPor}",
                registroId: id
            );

            // ── TWILIO ──
            Console.WriteLine($"📱 Teléfono conductor: '{s.Conductor?.Telefono}'");
            if (!string.IsNullOrWhiteSpace(s.Conductor?.Telefono))
            {
                var mensaje =
                    $"✅ *SOLICITUD TALLER AUTORIZADA*\n" +
                    $"Hola {s.Conductor.Nombre.Split(' ')[0]},\n" +
                    $"Tu solicitud de taller fue autorizada.\n" +
                    $"🚗 Vehículo: {s.Vehiculo?.Placa ?? "-"}\n" +
                    $"⚙️ Tipo: {s.TipoMantenimiento}\n" +
                    $"✍️ Autorizado por: {dto.AutorizadoPor}";
                await _twilio.EnviarMensajeAsync(s.Conductor.Telefono, mensaje);
            }
            else
                Console.WriteLine("⚠️ Conductor sin teléfono");

            return Ok(s);
        }

        [HttpPut("{id}/rechazar")]
        public async Task<IActionResult> Rechazar(int id, [FromBody] AutorizarSolicitudDto dto)
        {
            var s = await _context.SolicitudesTaller
                .Include(s => s.Conductor)
                .Include(s => s.Vehiculo)
                .FirstOrDefaultAsync(s => s.Id == id);
            if (s == null) return NotFound();

            s.Estado = "Rechazado";
            s.AutorizadoPor = dto.AutorizadoPor;
            s.ObservacionAut = dto.Observacion;
            s.FechaAutorizacion = DateTime.Now;
            await _context.SaveChangesAsync();

            await _auditoria.RegistrarAsync(
                usuario: GetUsuario(), rol: GetRol(),
                accion: "Rechazar", modulo: "SolicitudTaller",
                detalle: $"Solicitud #{id} RECHAZADA por: {dto.AutorizadoPor}",
                registroId: id
            );

            // ── TWILIO ──
            if (!string.IsNullOrWhiteSpace(s.Conductor?.Telefono))
            {
                var mensaje =
                    $"❌ *SOLICITUD TALLER RECHAZADA*\n" +
                    $"Hola {s.Conductor.Nombre.Split(' ')[0]},\n" +
                    $"Tu solicitud fue rechazada.\n" +
                    $"🚗 Vehículo: {s.Vehiculo?.Placa ?? "-"}\n" +
                    $"📋 Motivo: {dto.Observacion ?? "Sin observación"}";
                await _twilio.EnviarMensajeAsync(s.Conductor.Telefono, mensaje);
            }

            return Ok(s);
        }

        [HttpPut("{id}/en-taller")]
        public async Task<IActionResult> EnTaller(int id)
        {
            var s = await _context.SolicitudesTaller
                .Include(s => s.Conductor)
                .Include(s => s.Vehiculo)
                .FirstOrDefaultAsync(s => s.Id == id);
            if (s == null) return NotFound();

            s.Estado = "EnTaller";
            await _context.SaveChangesAsync();

            await _auditoria.RegistrarAsync(
                usuario: GetUsuario(), rol: GetRol(),
                accion: "Editar", modulo: "SolicitudTaller",
                detalle: $"Solicitud #{id} marcada EN TALLER",
                registroId: id
            );

            // ── TWILIO ──
            if (!string.IsNullOrWhiteSpace(s.Conductor?.Telefono))
            {
                var mensaje =
                    $"🔧 *VEHÍCULO EN TALLER*\n" +
                    $"Hola {s.Conductor.Nombre.Split(' ')[0]},\n" +
                    $"Tu vehículo {s.Vehiculo?.Placa ?? "-"} está siendo atendido en taller.";
                await _twilio.EnviarMensajeAsync(s.Conductor.Telefono, mensaje);
            }

            return Ok(s);
        }

        [HttpPut("{id}/factura")]
        public async Task<IActionResult> RegistrarFactura(int id, [FromBody] FacturaTallerDto dto)
        {
            var s = await _context.SolicitudesTaller
                .Include(s => s.Conductor)
                .Include(s => s.Vehiculo)
                .FirstOrDefaultAsync(s => s.Id == id);
            if (s == null) return NotFound();

            s.NumeroFacturaTaller = dto.NumeroFacturaTaller;
            s.ValorFactura = dto.ValorFactura;
            s.FechaFactura = dto.FechaFactura;
            s.FacturaValidada = dto.FacturaValidada;
            s.ObservacionFactura = dto.ObservacionFactura;
            if (dto.FacturaValidada) s.Estado = "Finalizado";
            await _context.SaveChangesAsync();

            await _auditoria.RegistrarAsync(
                usuario: GetUsuario(), rol: GetRol(),
                accion: "Editar", modulo: "SolicitudTaller",
                detalle: $"Factura taller #{id} — Nº: {dto.NumeroFacturaTaller}, Valor: {dto.ValorFactura}",
                registroId: id
            );

            // ── TWILIO ──
            if (dto.FacturaValidada && !string.IsNullOrWhiteSpace(s.Conductor?.Telefono))
            {
                var mensaje =
                    $"✅ *TALLER FINALIZADO*\n" +
                    $"Hola {s.Conductor.Nombre.Split(' ')[0]},\n" +
                    $"El mantenimiento de tu vehículo {s.Vehiculo?.Placa ?? "-"} fue completado.\n" +
                    $"💰 Valor: ${dto.ValorFactura?.ToString("N0") ?? "-"}\n" +
                    $"🧾 Factura: {dto.NumeroFacturaTaller ?? "-"}";
                await _twilio.EnviarMensajeAsync(s.Conductor.Telefono, mensaje);
            }

            return Ok(s);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var s = await _context.SolicitudesTaller.FindAsync(id);
            if (s == null) return NotFound();
            _context.SolicitudesTaller.Remove(s);
            await _context.SaveChangesAsync();
            await _auditoria.RegistrarAsync(
                usuario: GetUsuario(), rol: GetRol(),
                accion: "Eliminar", modulo: "SolicitudTaller",
                detalle: $"Solicitud taller #{id} eliminada",
                registroId: id
            );
            return Ok();
        }
    }

    public class AutorizarSolicitudDto
    {
        public string AutorizadoPor { get; set; } = string.Empty;
        public string? Observacion { get; set; }
    }

    public class FacturaTallerDto
    {
        public string? NumeroFacturaTaller { get; set; }
        public decimal? ValorFactura { get; set; }
        public DateTime? FechaFactura { get; set; }
        public bool FacturaValidada { get; set; }
        public string? ObservacionFactura { get; set; }
    }
}