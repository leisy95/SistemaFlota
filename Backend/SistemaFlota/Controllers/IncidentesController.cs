using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace SistemaFlota
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class IncidentesController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly AuditoriaService _auditoria;
        private readonly ITwilioService _twilio;

        public IncidentesController(
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
            var lista = await _context.Incidentes
                .Include(i => i.Conductor)
                .Include(i => i.Vehiculo)
                .Include(i => i.Autorizacion)
                .OrderByDescending(i => i.FechaReporte)
                .Select(i => new
                {
                    i.Id,
                    i.FechaReporte,
                    i.TipoIncidente,
                    i.DescripcionDetallada,
                    i.UbicacionGPS,
                    i.Latitud,
                    i.Longitud,
                    i.Fotos,
                    i.Estado,
                    i.FechaRevision,
                    i.RevisadoPor,
                    i.ObservacionRevision,
                    i.AutorizacionId,
                    Conductor = new { i.Conductor!.Id, i.Conductor.Nombre },
                    Vehiculo = new { i.Vehiculo!.Id, i.Vehiculo.Placa }
                })
                .ToListAsync();
            return Ok(lista);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var incidente = await _context.Incidentes
                .Include(i => i.Conductor)
                .Include(i => i.Vehiculo)
                .Include(i => i.Autorizacion)
                .Where(i => i.Id == id)
                .Select(i => new
                {
                    i.Id,
                    i.FechaReporte,
                    i.TipoIncidente,
                    i.DescripcionDetallada,
                    i.UbicacionGPS,
                    i.Latitud,
                    i.Longitud,
                    i.Fotos,
                    i.Estado,
                    i.FechaRevision,
                    i.RevisadoPor,
                    i.ObservacionRevision,
                    i.AutorizacionId,
                    Conductor = new { i.Conductor!.Id, i.Conductor.Nombre },
                    Vehiculo = new { i.Vehiculo!.Id, i.Vehiculo.Placa }
                })
                .FirstOrDefaultAsync();
            if (incidente == null) return NotFound();
            return Ok(incidente);
        }

        [HttpPost]
        public async Task<IActionResult> Post(
            [FromForm] int ConductorId,
            [FromForm] int VehiculoId,
            [FromForm] int? AutorizacionId,
            [FromForm] string TipoIncidente,
            [FromForm] string DescripcionDetallada,
            [FromForm] string? UbicacionGPS,
            [FromForm] double? Latitud,
            [FromForm] double? Longitud,
            [FromForm] List<IFormFile> Fotos)
        {
            try
            {
                var carpeta = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/incidentes");
                if (!Directory.Exists(carpeta)) Directory.CreateDirectory(carpeta);

                var nombresfotos = new List<string>();
                foreach (var foto in Fotos.Take(5))
                {
                    var nombre = Guid.NewGuid().ToString() + Path.GetExtension(foto.FileName);
                    using var stream = new FileStream(Path.Combine(carpeta, nombre), FileMode.Create);
                    await foto.CopyToAsync(stream);
                    nombresfotos.Add(nombre);
                }

                var incidente = new Incidente
                {
                    ConductorId = ConductorId,
                    VehiculoId = VehiculoId,
                    AutorizacionId = AutorizacionId,
                    TipoIncidente = TipoIncidente,
                    DescripcionDetallada = DescripcionDetallada,
                    UbicacionGPS = UbicacionGPS,
                    Latitud = Latitud,
                    Longitud = Longitud,
                    Fotos = string.Join(",", nombresfotos),
                    Estado = "Pendiente",
                    FechaReporte = DateTime.Now
                };

                _context.Incidentes.Add(incidente);
                await _context.SaveChangesAsync();

                var conductor = await _context.Conductores.FindAsync(ConductorId);
                var vehiculo = await _context.Vehiculos.FindAsync(VehiculoId);

                await _auditoria.RegistrarAsync(
                    usuario: GetUsuario(), rol: GetRol(),
                    accion: "Crear", modulo: "Incidentes",
                    detalle: $"Incidente reportado — Conductor: {conductor?.Nombre ?? "-"}, Vehículo: {vehiculo?.Placa ?? "-"}, Tipo: {TipoIncidente}",
                    registroId: incidente.Id
                );

                // ── TWILIO ──
                var hora = DateTime.Now.ToString("hh:mm tt");
                var fecha = DateTime.Now.ToString("dd/MM/yyyy");
                var mensajeGrupo =
                    $"🚨 *INCIDENTE REPORTADO*\n" +
                    $"👤 Conductor: {conductor?.Nombre ?? "-"}\n" +
                    $"🚗 Vehículo: {vehiculo?.Placa ?? "-"}\n" +
                    $"⚠️ Tipo: {TipoIncidente}\n" +
                    $"📋 Descripción: {DescripcionDetallada}\n" +
                    $"📍 Ubicación: {UbicacionGPS ?? "No especificada"}\n" +
                    $"🕐 Hora: {hora} — {fecha}";

                var numerosGrupo = await _context.ContactosNotificacion
                    .Where(c => c.Activo && c.RecibeIncidentes)
                    .Select(c => c.NumeroWhatsApp)
                    .ToListAsync();

                Console.WriteLine($"📱 Contactos grupo: {numerosGrupo.Count}");
                if (numerosGrupo.Any())
                    await _twilio.EnviarAMultiplesAsync(numerosGrupo, mensajeGrupo);

                return Ok(incidente);
            }
            catch (Exception ex)
            {
                await _auditoria.RegistrarAsync(
                    usuario: GetUsuario(), rol: GetRol(),
                    accion: "Crear", modulo: "Incidentes",
                    detalle: $"Error: {ex.Message}", resultado: "Fallido"
                );
                return StatusCode(500, ex.InnerException?.Message ?? ex.Message);
            }
        }

        [HttpPut("{id}/revisar")]
        public async Task<IActionResult> Revisar(int id, [FromBody] RevisarIncidenteDto dto)
        {
            var incidente = await _context.Incidentes
                .Include(i => i.Conductor)
                .Include(i => i.Vehiculo)
                .FirstOrDefaultAsync(i => i.Id == id);
            if (incidente == null) return NotFound();

            incidente.Estado = "Revisado";
            incidente.FechaRevision = DateTime.Now;
            incidente.RevisadoPor = dto.RevisadoPor;
            incidente.ObservacionRevision = dto.Observacion;
            await _context.SaveChangesAsync();

            await _auditoria.RegistrarAsync(
                usuario: GetUsuario(), rol: GetRol(),
                accion: "Revisar", modulo: "Incidentes",
                detalle: $"Incidente #{id} marcado como revisado por: {dto.RevisadoPor}",
                registroId: id
            );

            // ── TWILIO ──
            Console.WriteLine($"📱 Teléfono conductor: '{incidente.Conductor?.Telefono}'");
            if (!string.IsNullOrWhiteSpace(incidente.Conductor?.Telefono))
            {
                var mensaje =
                    $"✅ *INCIDENTE REVISADO*\n" +
                    $"Hola {incidente.Conductor.Nombre.Split(' ')[0]},\n" +
                    $"Tu reporte de incidente #{id} fue revisado.\n" +
                    $"🚗 Vehículo: {incidente.Vehiculo?.Placa ?? "-"}\n" +
                    $"👤 Revisado por: {dto.RevisadoPor}\n" +
                    $"📋 Observación: {dto.Observacion ?? "Sin observación"}";
                await _twilio.EnviarMensajeAsync(incidente.Conductor.Telefono, mensaje);
            }
            else
                Console.WriteLine("⚠️ Conductor sin teléfono");

            return Ok(incidente);
        }

        [HttpGet("contactos-whatsapp")]
        public async Task<IActionResult> GetContactos()
        {
            var contactos = await _context.ContactosNotificacion
                .Where(c => c.Activo && c.RecibeIncidentes)
                .ToListAsync();
            return Ok(contactos);
        }
    }

    public class RevisarIncidenteDto
    {
        public string RevisadoPor { get; set; } = string.Empty;
        public string? Observacion { get; set; }
    }
}