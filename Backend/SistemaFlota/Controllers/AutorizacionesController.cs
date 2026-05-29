using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace SistemaFlota
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AutorizacionesController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly AuditoriaService _auditoria;

        public AutorizacionesController(AppDbContext context, AuditoriaService auditoria)
        {
            _context = context;
            _auditoria = auditoria;
        }

        private string GetUsuario() =>
            User.FindFirst(ClaimTypes.Name)?.Value ?? "Desconocido";
        private string GetRol() =>
            User.FindFirst(ClaimTypes.Role)?.Value ?? "Desconocido";

        private async Task<Autorizacion?> CargarConRelaciones(int id) =>
            await _context.Autorizaciones
                .Include(a => a.Conductor)
                .Include(a => a.Vehiculo)
                .FirstOrDefaultAsync(a => a.Id == id);

        // =====================================
        // GET TODAS
        // =====================================
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            try
            {
                var lista = await _context.Autorizaciones
                    .Include(a => a.Conductor)
                    .Include(a => a.Vehiculo)
                    .OrderByDescending(a => a.FechaCreacion)
                    .ToListAsync();
                return Ok(lista);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ ERROR GET Autorizaciones: {ex.Message}");
                return StatusCode(500, ex.InnerException?.Message ?? ex.Message);
            }
        }

        // =====================================
        // GET — GENERAR NÚMERO DE GUÍA
        // =====================================
        [HttpGet("generar-guia")]
        public async Task<IActionResult> GenerarGuia()
        {
            try
            {
                var total = await _context.Autorizaciones
                    .Where(a => a.NumeroGuia != null && a.NumeroGuia.StartsWith("GI-"))
                    .CountAsync();
                var consecutivo = (total + 1).ToString("D4");
                var fecha = DateTime.Now.ToString("yyyyMMdd");
                return Ok(new { guia = $"GI-{fecha}-{consecutivo}" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.InnerException?.Message ?? ex.Message);
            }
        }

        // =====================================
        // GET POR ID
        // =====================================
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var a = await CargarConRelaciones(id);
                if (a == null) return NotFound();
                return Ok(a);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.InnerException?.Message ?? ex.Message);
            }
        }

        // =====================================
        // POST — CREAR
        // =====================================
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] CrearAutorizacionDto dto)
        {
            try
            {
                var autorizacion = new Autorizacion
                {
                    ConductorId = dto.ConductorId,
                    VehiculoId = dto.VehiculoId,
                    DestinoCompleto = dto.DestinoCompleto ?? string.Empty,
                    CantidadClientes = dto.CantidadClientes,
                    PesoKilos = dto.PesoKilos,
                    TipoVuelta = dto.TipoVuelta ?? string.Empty,
                    DescripcionCarga = dto.DescripcionCarga ?? string.Empty,
                    NumeroGuia = dto.NumeroGuia,
                    FacturasClientes = dto.FacturasClientes,
                    Estado = "Pendiente",
                    FechaCreacion = DateTime.Now
                };

                _context.Autorizaciones.Add(autorizacion);
                await _context.SaveChangesAsync();

                var resultado = await CargarConRelaciones(autorizacion.Id);

                await _auditoria.RegistrarAsync(
                    usuario: GetUsuario(), rol: GetRol(),
                    accion: "Crear", modulo: "Autorizaciones",
                    detalle: $"Autorización creada — Conductor: {resultado?.Conductor?.Nombre ?? "-"}, Vehículo: {resultado?.Vehiculo?.Placa ?? "-"}",
                    registroId: autorizacion.Id
                );

                return Ok(resultado);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ ERROR POST Autorizacion: {ex.Message}\n{ex.InnerException?.Message}");
                return StatusCode(500, ex.InnerException?.Message ?? ex.Message);
            }
        }

        // =====================================
        // PUT — FIRMA FACTURACIÓN
        // =====================================
        [HttpPut("{id}/facturacion")]
        public async Task<IActionResult> FirmarFacturacion(int id, [FromBody] FirmaDto dto)
        {
            try
            {
                var a = await _context.Autorizaciones.FindAsync(id);
                if (a == null) return NotFound();

                a.FirmaFacturacion = dto.Firma;
                a.UsuarioFacturacion = dto.Usuario;
                a.ObservacionFacturacion = dto.Observacion;
                a.FechaFacturacion = DateTime.Now;
                a.Estado = "Bodega";

                await _context.SaveChangesAsync();

                await _auditoria.RegistrarAsync(
                    usuario: GetUsuario(), rol: GetRol(),
                    accion: "Firmar", modulo: "Autorizaciones",
                    detalle: $"Firma Facturación — #{id}, por: {dto.Usuario}",
                    registroId: id
                );

                return Ok(await CargarConRelaciones(id));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.InnerException?.Message ?? ex.Message);
            }
        }

        // =====================================
        // PUT — FIRMA BODEGA
        // =====================================
        [HttpPut("{id}/bodega")]
        public async Task<IActionResult> FirmarBodega(int id, [FromBody] FirmaDto dto)
        {
            try
            {
                var a = await _context.Autorizaciones.FindAsync(id);
                if (a == null) return NotFound();

                a.FirmaBodega = dto.Firma;
                a.UsuarioBodega = dto.Usuario;
                a.ObservacionBodega = dto.Observacion;
                a.FechaBodega = DateTime.Now;
                a.Estado = "Porteria";

                await _context.SaveChangesAsync();

                await _auditoria.RegistrarAsync(
                    usuario: GetUsuario(), rol: GetRol(),
                    accion: "Firmar", modulo: "Autorizaciones",
                    detalle: $"Firma Bodega — #{id}, por: {dto.Usuario}",
                    registroId: id
                );

                return Ok(await CargarConRelaciones(id));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.InnerException?.Message ?? ex.Message);
            }
        }

        // =====================================
        // PUT — FIRMA PORTERÍA SALIDA
        // =====================================
        [HttpPut("{id}/porteria")]
        public async Task<IActionResult> FirmarPorteria(int id, [FromBody] FirmaDto dto)
        {
            try
            {
                var a = await _context.Autorizaciones.FindAsync(id);
                if (a == null) return NotFound();

                a.FirmaPorteria = dto.Firma;
                a.UsuarioPorteria = dto.Usuario;
                a.ObservacionPorteria = dto.Observacion;
                a.FechaPorteria = DateTime.Now;
                a.Estado = "Autorizado";
                a.EstadoLlegada = null; // ← en ruta, aún no ha llegado

                await _context.SaveChangesAsync();

                await _auditoria.RegistrarAsync(
                    usuario: GetUsuario(), rol: GetRol(),
                    accion: "Firmar", modulo: "Autorizaciones",
                    detalle: $"Firma Portería Salida — #{id} AUTORIZADO, por: {dto.Usuario}",
                    registroId: id
                );

                return Ok(await CargarConRelaciones(id));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.InnerException?.Message ?? ex.Message);
            }
        }

        // =====================================
        // PUT — REPORTE LLEGADA (CONDUCTOR)
        // El conductor reporta que llegó a la empresa
        // =====================================
        [HttpPut("{id}/reportar-llegada")]
        public async Task<IActionResult> ReportarLlegada(int id, [FromBody] LlegadaConductorDto dto)
        {
            try
            {
                var a = await _context.Autorizaciones.FindAsync(id);
                if (a == null) return NotFound();

                // Solo se puede reportar llegada si ya fue autorizada la salida
                if (a.Estado != "Autorizado")
                    return BadRequest("Solo se puede reportar llegada de autorizaciones en estado Autorizado");

                // Solo se puede reportar una vez
                if (a.EstadoLlegada == "ReportadaLlegada" || a.EstadoLlegada == "Completada")
                    return BadRequest("La llegada ya fue reportada");

                a.FechaReporteLlegada = DateTime.Now;
                a.KilometrajeFinal = dto.KilometrajeFinal;
                a.NovedadesViaje = dto.NovedadesViaje;
                a.EstadoVehiculoLlegada = dto.EstadoVehiculo; // Bueno | Novedad | Requiere taller
                a.EstadoLlegada = "ReportadaLlegada";

                await _context.SaveChangesAsync();

                await _auditoria.RegistrarAsync(
                    usuario: GetUsuario(), rol: GetRol(),
                    accion: "ReportarLlegada", modulo: "Autorizaciones",
                    detalle: $"Llegada reportada por conductor — #{id}, Km final: {dto.KilometrajeFinal}, Estado vehículo: {dto.EstadoVehiculo}",
                    registroId: id
                );

                return Ok(await CargarConRelaciones(id));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ ERROR ReportarLlegada: {ex.Message}");
                return StatusCode(500, ex.InnerException?.Message ?? ex.Message);
            }
        }

        // =====================================
        // PUT — CONFIRMAR LLEGADA (PORTERÍA)
        // Portería confirma que el vehículo entró físicamente
        // =====================================
        [HttpPut("{id}/confirmar-llegada")]
        public async Task<IActionResult> ConfirmarLlegada(int id, [FromBody] FirmaDto dto)
        {
            try
            {
                var a = await _context.Autorizaciones.FindAsync(id);
                if (a == null) return NotFound();

                // Solo se puede confirmar si el conductor ya reportó
                if (a.EstadoLlegada != "ReportadaLlegada")
                    return BadRequest("El conductor aún no ha reportado la llegada");

                a.FechaConfirmacionLlegada = DateTime.Now;
                a.UsuarioPorteriaLlegada = dto.Usuario;
                a.ObservacionPorteriaLlegada = dto.Observacion;
                a.FirmaPorteriaLlegada = dto.Firma;
                a.EstadoLlegada = "Completada";

                await _context.SaveChangesAsync();

                await _auditoria.RegistrarAsync(
                    usuario: GetUsuario(), rol: GetRol(),
                    accion: "ConfirmarLlegada", modulo: "Autorizaciones",
                    detalle: $"Llegada confirmada por portería — #{id}, por: {dto.Usuario}",
                    registroId: id
                );

                return Ok(await CargarConRelaciones(id));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ ERROR ConfirmarLlegada: {ex.Message}");
                return StatusCode(500, ex.InnerException?.Message ?? ex.Message);
            }
        }

        // =====================================
        // PUT — RECHAZAR
        // =====================================
        [HttpPut("{id}/rechazar")]
        public async Task<IActionResult> Rechazar(int id, [FromBody] FirmaDto dto)
        {
            try
            {
                var a = await _context.Autorizaciones.FindAsync(id);
                if (a == null) return NotFound();

                a.Estado = "Rechazado";
                await _context.SaveChangesAsync();

                await _auditoria.RegistrarAsync(
                    usuario: GetUsuario(), rol: GetRol(),
                    accion: "Rechazar", modulo: "Autorizaciones",
                    detalle: $"Autorización #{id} RECHAZADA",
                    registroId: id
                );

                return Ok(await CargarConRelaciones(id));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.InnerException?.Message ?? ex.Message);
            }
        }
    }

    // =====================================
    // DTOs
    // =====================================
    public class CrearAutorizacionDto
    {
        public int ConductorId { get; set; }
        public int VehiculoId { get; set; }
        public string DestinoCompleto { get; set; } = string.Empty;
        public int CantidadClientes { get; set; }
        public decimal PesoKilos { get; set; }
        public string TipoVuelta { get; set; } = string.Empty;
        public string DescripcionCarga { get; set; } = string.Empty;
        public string? NumeroGuia { get; set; }
        public string? FacturasClientes { get; set; }
    }

    public class FirmaDto
    {
        public string Firma { get; set; } = string.Empty;
        public string Usuario { get; set; } = string.Empty;
        public string? Observacion { get; set; }
    }

    // DTO específico para el reporte de llegada del conductor
    public class LlegadaConductorDto
    {
        public int? KilometrajeFinal { get; set; }
        public string? NovedadesViaje { get; set; }
        // Bueno | Novedad | Requiere taller
        public string EstadoVehiculo { get; set; } = "Bueno";
    }
}