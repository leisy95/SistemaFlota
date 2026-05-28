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

        public AutorizacionesController(
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

        // ── Helper: carga una autorización con todas sus relaciones ──────────
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
        // GET — GENERAR NÚMERO DE GUÍA INTERNA
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
                var guia = $"GI-{fecha}-{consecutivo}";

                return Ok(new { guia });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ ERROR GenerarGuia: {ex.Message}");
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
                var autorizacion = await CargarConRelaciones(id);
                if (autorizacion == null) return NotFound();
                return Ok(autorizacion);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ ERROR GetById Autorizacion: {ex.Message}");
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
                Console.WriteLine($"📥 POST Autorizacion — ConductorId: {dto.ConductorId}, VehiculoId: {dto.VehiculoId}, Tipo: {dto.TipoVuelta}");

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

                Console.WriteLine($"✅ Autorizacion creada ID: {autorizacion.Id}");

                // ── Recargar con Conductor y Vehiculo incluidos ──────────────
                var resultado = await CargarConRelaciones(autorizacion.Id);

                await _auditoria.RegistrarAsync(
                    usuario: GetUsuario(),
                    rol: GetRol(),
                    accion: "Crear",
                    modulo: "Autorizaciones",
                    detalle: $"Autorización creada — Conductor: {resultado?.Conductor?.Nombre ?? "-"}, Vehículo: {resultado?.Vehiculo?.Placa ?? "-"}, Tipo: {dto.TipoVuelta}",
                    registroId: autorizacion.Id
                );

                return Ok(resultado);  // ← devuelve objeto COMPLETO con relaciones
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ ERROR POST Autorizacion: {ex.Message}");
                Console.WriteLine($"❌ INNER: {ex.InnerException?.Message}");
                Console.WriteLine($"❌ STACK: {ex.StackTrace}");
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
                var autorizacion = await _context.Autorizaciones.FindAsync(id);
                if (autorizacion == null) return NotFound();

                autorizacion.FirmaFacturacion = dto.Firma;
                autorizacion.UsuarioFacturacion = dto.Usuario;
                autorizacion.ObservacionFacturacion = dto.Observacion;
                autorizacion.FechaFacturacion = DateTime.Now;
                autorizacion.Estado = "Bodega";

                await _context.SaveChangesAsync();

                await _auditoria.RegistrarAsync(
                    usuario: GetUsuario(),
                    rol: GetRol(),
                    accion: "Firmar",
                    modulo: "Autorizaciones",
                    detalle: $"Firma Facturación — Autorización #{id}, Firmado por: {dto.Usuario}",
                    registroId: id
                );

                // ── Devolver con relaciones ──────────────────────────────────
                return Ok(await CargarConRelaciones(id));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ ERROR FirmarFacturacion: {ex.Message}");
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
                var autorizacion = await _context.Autorizaciones.FindAsync(id);
                if (autorizacion == null) return NotFound();

                autorizacion.FirmaBodega = dto.Firma;
                autorizacion.UsuarioBodega = dto.Usuario;
                autorizacion.ObservacionBodega = dto.Observacion;
                autorizacion.FechaBodega = DateTime.Now;
                autorizacion.Estado = "Porteria";

                await _context.SaveChangesAsync();

                await _auditoria.RegistrarAsync(
                    usuario: GetUsuario(),
                    rol: GetRol(),
                    accion: "Firmar",
                    modulo: "Autorizaciones",
                    detalle: $"Firma Bodega — Autorización #{id}, Firmado por: {dto.Usuario}",
                    registroId: id
                );

                // ── Devolver con relaciones ──────────────────────────────────
                return Ok(await CargarConRelaciones(id));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ ERROR FirmarBodega: {ex.Message}");
                return StatusCode(500, ex.InnerException?.Message ?? ex.Message);
            }
        }

        // =====================================
        // PUT — FIRMA PORTERÍA
        // =====================================
        [HttpPut("{id}/porteria")]
        public async Task<IActionResult> FirmarPorteria(int id, [FromBody] FirmaDto dto)
        {
            try
            {
                var autorizacion = await _context.Autorizaciones.FindAsync(id);
                if (autorizacion == null) return NotFound();

                autorizacion.FirmaPorteria = dto.Firma;
                autorizacion.UsuarioPorteria = dto.Usuario;
                autorizacion.ObservacionPorteria = dto.Observacion;
                autorizacion.FechaPorteria = DateTime.Now;
                autorizacion.Estado = "Autorizado";

                await _context.SaveChangesAsync();

                await _auditoria.RegistrarAsync(
                    usuario: GetUsuario(),
                    rol: GetRol(),
                    accion: "Firmar",
                    modulo: "Autorizaciones",
                    detalle: $"Firma Portería — Autorización #{id} AUTORIZADA, Firmado por: {dto.Usuario}",
                    registroId: id
                );

                // ── Devolver con relaciones ──────────────────────────────────
                return Ok(await CargarConRelaciones(id));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ ERROR FirmarPorteria: {ex.Message}");
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
                var autorizacion = await _context.Autorizaciones.FindAsync(id);
                if (autorizacion == null) return NotFound();

                autorizacion.Estado = "Rechazado";
                await _context.SaveChangesAsync();

                await _auditoria.RegistrarAsync(
                    usuario: GetUsuario(),
                    rol: GetRol(),
                    accion: "Rechazar",
                    modulo: "Autorizaciones",
                    detalle: $"Autorización #{id} RECHAZADA",
                    registroId: id
                );

                return Ok(await CargarConRelaciones(id));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ ERROR Rechazar: {ex.Message}");
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
}