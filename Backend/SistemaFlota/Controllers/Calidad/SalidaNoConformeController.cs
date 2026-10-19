using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaFlota.Authorization;
using SistemaFlota.DTOs;
using SistemaFlota.Services.Calidad;
using System.Security.Claims;

namespace SistemaFlota.Controllers.Calidad
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class SalidaNoConformeController : ControllerBase
    {
        private readonly ISalidaNoConformeService _service;
        private readonly AppDbContext _context;
        private readonly AuditoriaService _auditoria;

        public SalidaNoConformeController(ISalidaNoConformeService service, AppDbContext context, AuditoriaService auditoria)
        {
            _service = service;
            _context = context;
            _auditoria = auditoria;
        }

        private string GetUsuario() => User.FindFirst(ClaimTypes.Name)?.Value ?? "Desconocido";
        private string GetRol() => User.FindFirst(ClaimTypes.Role)?.Value ?? "Desconocido";
        private bool EsAdmin() => GetRol() == "Admin";

        [HttpGet]
        [Permiso("gestion-snc", "ver")]
        public async Task<IActionResult> Get(
            [FromQuery] DateTime? desde, [FromQuery] DateTime? hasta,
            [FromQuery] string? referencia, [FromQuery] string? material,
            [FromQuery] string? ordenProduccion, [FromQuery] string? tipoDefecto,
            [FromQuery] string? proceso)
        {
            var lista = await _service.ListarAsync(desde, hasta, referencia, material, ordenProduccion, tipoDefecto, proceso);
            return Ok(lista);
        }

        [HttpGet("{id}")]
        [Permiso("gestion-snc", "ver")]
        public async Task<IActionResult> GetPorId(int id)
        {
            var entidad = await _service.ObtenerAsync(id);
            if (entidad == null) return NotFound();
            return Ok(entidad);
        }

        // Quien puede fijar la fecha real de un reporte: Admin o permiso de editar en gestion-snc, y nunca las cuentas de máquina
        private async Task<bool> PuedeFijarFechaAsync()
        {
            var cuentasMaquina = new[] { "Extrusion", "Precorte", "Impresion", "Sellado" };
            if (cuentasMaquina.Contains(GetUsuario(), StringComparer.OrdinalIgnoreCase)) return false;
            if (EsAdmin()) return true;

            int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var usuarioId);
            return await _context.UsuarioPermisos.AnyAsync(p =>
                p.UsuarioId == usuarioId && p.Modulo == "gestion-snc" && p.PuedeEditar);
        }

        [HttpPost]
        [Permiso("gestion-snc", "crear")]
        public async Task<IActionResult> Post([FromForm] CrearSalidaNoConformeDto dto, IFormFile? evidenciaPdf)
        {
            var puedeFijarFecha = await PuedeFijarFechaAsync();

            if (dto.FechaRealReporte.HasValue && !puedeFijarFecha)
                return StatusCode(403, new { mensaje = "No tiene permiso para fijar la fecha real del reporte" });

            if (dto.FechaRealReporte.HasValue && dto.FechaRealReporte.Value.Date > DateTime.Now.Date)
                return BadRequest(new { mensaje = "La fecha real del reporte no puede ser futura" });

            var entidad = await _service.CrearAsync(dto, GetUsuario(), evidenciaPdf, puedeFijarFecha);

            if (dto.FechaRealReporte.HasValue)
                await _auditoria.RegistrarAsync(GetUsuario(), GetRol(), "Fecha-Manual", "SalidaNoConforme",
                    $"OP {entidad.OrdenProduccion}: fecha real del reporte {entidad.FechaReporte:dd/MM/yyyy}, registrada el {DateTime.Now:dd/MM/yyyy HH:mm}",
                    entidad.Id);

            return Ok(entidad);
        }

        [HttpPut("{id}/tratamiento")]
        [Permiso("gestion-snc", "editar")]
        public async Task<IActionResult> Tratamiento(int id, [FromBody] TratamientoSncDto dto)
        {
            var entidad = await _service.RegistrarTratamientoAsync(id, dto, GetUsuario());
            if (entidad == null) return NotFound();
            return Ok(entidad);
        }

        [HttpPut("{id}/cerrar")]
        [Permiso("gestion-snc", "editar")]
        public async Task<IActionResult> Cerrar(int id, [FromForm] VerificacionSncDto dto, IFormFile? evidenciaPdf)
        {
            var entidad = await _service.CerrarAsync(id, dto, evidenciaPdf);
            if (entidad == null) return NotFound();
            return Ok(entidad);
        }

        [HttpDelete("{id}")]
        [Permiso("gestion-snc", "eliminar")]
        public async Task<IActionResult> Delete(int id)
        {
            var eliminado = await _service.EliminarAsync(id, EsAdmin());
            if (!eliminado) return Forbid();
            return Ok();
        }

        [HttpPost("{id}/evidencias")]
        [Permiso("gestion-snc", "crear")]
        public async Task<IActionResult> SubirEvidencias(int id, [FromForm] string paso, [FromForm] List<IFormFile> fotos)
        {
            var evidencias = await _service.SubirEvidenciasAsync(id, paso, fotos);
            return Ok(evidencias);
        }

        [HttpGet("{id}/evidencias")]
        [Permiso("gestion-snc", "ver")]
        public async Task<IActionResult> GetEvidencias(int id)
        {
            var evidencias = await _service.ObtenerEvidenciasAsync(id);
            return Ok(evidencias);
        }

        [HttpGet("suma-filtrada")]
        [Permiso("gestion-snc", "ver")]
        public async Task<IActionResult> SumaFiltrada(
            [FromQuery] DateTime? desde, [FromQuery] DateTime? hasta,
            [FromQuery] string? referencia, [FromQuery] string? material,
            [FromQuery] string? ordenProduccion, [FromQuery] string? tipoDefecto,
            [FromQuery] string? proceso)
        {
            var registros = await _service.ListarAsync(desde, hasta, referencia, material, ordenProduccion, tipoDefecto, proceso);
            var total = registros.Sum(r => r.CantidadReportadaKg ?? 0);
            return Ok(new { total, totalRegistros = registros.Count });
        }

        [HttpGet("kg-del-mes")]
        [Permiso("gestion-snc", "ver")]
        public async Task<IActionResult> KgDelMes([FromQuery] int mes, [FromQuery] int anio)
        {
            var registros = await _service.ListarAsync(null, null, null, null, null, null, null);
            var total = _service.SumarKgDelMes(registros, mes, anio);
            return Ok(new { mes, anio, totalKg = total });
        }
    }
}