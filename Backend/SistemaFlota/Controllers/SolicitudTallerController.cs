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

        public SolicitudTallerController(
            AppDbContext context,
            AuditoriaService auditoria)
        {
            _context = context;
            _auditoria = auditoria;
        }

        private string GetUsuario() =>
            User.FindFirst(ClaimTypes.Name)?.Value ?? "Desconocido";
        private string GetRol() =>
            User.FindFirst(ClaimTypes.Role)?.Value ?? "Desconocido";

        // =====================================
        // GET TODAS
        // =====================================

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
                    Conductor = new
                    {
                        s.Conductor!.Id,
                        s.Conductor.Nombre,
                        s.Conductor.Telefono
                    },
                    Vehiculo = new
                    {
                        s.Vehiculo!.Id,
                        s.Vehiculo.Placa,
                        s.Vehiculo.Marca,
                        s.Vehiculo.Modelo
                    }
                })
                .ToListAsync();

            return Ok(lista);
        }

        // =====================================
        // GET POR ID
        // =====================================

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

        // =====================================
        // POST — CREAR SOLICITUD
        // =====================================

        [HttpPost]
        public async Task<IActionResult> Post(
            [FromForm] int ConductorId,
            [FromForm] int VehiculoId,
            [FromForm] string TipoMantenimiento,
            [FromForm] string DescripcionProblema,
            [FromForm] int? Kilometraje,
            IFormFile? FotoOdometro
        )
        {
            try
            {
                string? nombreFoto = null;

                if (FotoOdometro != null)
                {
                    var carpeta = Path.Combine(
                        Directory.GetCurrentDirectory(),
                        "wwwroot/solicitudes-taller"
                    );

                    if (!Directory.Exists(carpeta))
                        Directory.CreateDirectory(carpeta);

                    nombreFoto = Guid.NewGuid().ToString()
                        + Path.GetExtension(FotoOdometro.FileName);

                    using var stream = new FileStream(
                        Path.Combine(carpeta, nombreFoto), FileMode.Create);
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
                    usuario: GetUsuario(),
                    rol: GetRol(),
                    accion: "Crear",
                    modulo: "SolicitudTaller",
                    detalle: $"Solicitud taller — Conductor: {conductor?.Nombre ?? "-"}, Vehículo: {vehiculo?.Placa ?? "-"}, Tipo: {TipoMantenimiento}",
                    registroId: solicitud.Id
                );

                return Ok(solicitud);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.InnerException?.Message ?? ex.Message);
            }
        }

        // =====================================
        // PUT — AUTORIZAR SALIDA
        // =====================================

        [HttpPut("{id}/autorizar")]
        public async Task<IActionResult> Autorizar(
            int id,
            [FromBody] AutorizarSolicitudDto dto)
        {
            var s = await _context.SolicitudesTaller.FindAsync(id);
            if (s == null) return NotFound();

            s.Estado = "Autorizado";
            s.AutorizadoPor = dto.AutorizadoPor;
            s.ObservacionAut = dto.Observacion;
            s.FechaAutorizacion = DateTime.Now;

            await _context.SaveChangesAsync();

            await _auditoria.RegistrarAsync(
                usuario: GetUsuario(),
                rol: GetRol(),
                accion: "Autorizar",
                modulo: "SolicitudTaller",
                detalle: $"Solicitud #{id} AUTORIZADA por: {dto.AutorizadoPor}",
                registroId: id
            );

            return Ok(s);
        }

        // =====================================
        // PUT — RECHAZAR
        // =====================================

        [HttpPut("{id}/rechazar")]
        public async Task<IActionResult> Rechazar(
            int id,
            [FromBody] AutorizarSolicitudDto dto)
        {
            var s = await _context.SolicitudesTaller.FindAsync(id);
            if (s == null) return NotFound();

            s.Estado = "Rechazado";
            s.AutorizadoPor = dto.AutorizadoPor;
            s.ObservacionAut = dto.Observacion;
            s.FechaAutorizacion = DateTime.Now;

            await _context.SaveChangesAsync();

            await _auditoria.RegistrarAsync(
                usuario: GetUsuario(),
                rol: GetRol(),
                accion: "Rechazar",
                modulo: "SolicitudTaller",
                detalle: $"Solicitud #{id} RECHAZADA por: {dto.AutorizadoPor}",
                registroId: id
            );

            return Ok(s);
        }

        // =====================================
        // PUT — MARCAR EN TALLER
        // =====================================

        [HttpPut("{id}/en-taller")]
        public async Task<IActionResult> EnTaller(int id)
        {
            var s = await _context.SolicitudesTaller.FindAsync(id);
            if (s == null) return NotFound();

            s.Estado = "EnTaller";
            await _context.SaveChangesAsync();

            await _auditoria.RegistrarAsync(
                usuario: GetUsuario(),
                rol: GetRol(),
                accion: "Editar",
                modulo: "SolicitudTaller",
                detalle: $"Solicitud #{id} marcada EN TALLER",
                registroId: id
            );

            return Ok(s);
        }

        // =====================================
        // PUT — REGISTRAR FACTURA TALLER
        // =====================================

        [HttpPut("{id}/factura")]
        public async Task<IActionResult> RegistrarFactura(
            int id,
            [FromBody] FacturaTallerDto dto)
        {
            var s = await _context.SolicitudesTaller.FindAsync(id);
            if (s == null) return NotFound();

            s.NumeroFacturaTaller = dto.NumeroFacturaTaller;
            s.ValorFactura = dto.ValorFactura;
            s.FechaFactura = dto.FechaFactura;
            s.FacturaValidada = dto.FacturaValidada;
            s.ObservacionFactura = dto.ObservacionFactura;

            if (dto.FacturaValidada)
                s.Estado = "Finalizado";

            await _context.SaveChangesAsync();

            await _auditoria.RegistrarAsync(
                usuario: GetUsuario(),
                rol: GetRol(),
                accion: "Editar",
                modulo: "SolicitudTaller",
                detalle: $"Factura taller #{id} registrada — Nº: {dto.NumeroFacturaTaller}, Valor: {dto.ValorFactura}",
                registroId: id
            );

            return Ok(s);
        }

        // =====================================
        // DELETE
        // =====================================

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var s = await _context.SolicitudesTaller.FindAsync(id);
            if (s == null) return NotFound();

            _context.SolicitudesTaller.Remove(s);
            await _context.SaveChangesAsync();

            await _auditoria.RegistrarAsync(
                usuario: GetUsuario(),
                rol: GetRol(),
                accion: "Eliminar",
                modulo: "SolicitudTaller",
                detalle: $"Solicitud taller #{id} eliminada",
                registroId: id
            );

            return Ok();
        }
    }

    // =====================================
    // DTOs
    // =====================================

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