using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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

        public SalidaNoConformeController(ISalidaNoConformeService service)
        {
            _service = service;
        }

        private string GetUsuario() => User.FindFirst(ClaimTypes.Name)?.Value ?? "Desconocido";
        private bool EsAdmin() => User.FindFirst(ClaimTypes.Role)?.Value == "Admin";

        [HttpGet]
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
        public async Task<IActionResult> GetPorId(int id)
        {
            var entidad = await _service.ObtenerAsync(id);
            if (entidad == null) return NotFound();
            return Ok(entidad);
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromForm] CrearSalidaNoConformeDto dto, IFormFile? evidenciaPdf)
        {
            var entidad = await _service.CrearAsync(dto, GetUsuario(), evidenciaPdf);
            return Ok(entidad);
        }

        [HttpPut("{id}/tratamiento")]
        public async Task<IActionResult> Tratamiento(int id, [FromBody] TratamientoSncDto dto)
        {
            var entidad = await _service.RegistrarTratamientoAsync(id, dto, GetUsuario());
            if (entidad == null) return NotFound();
            return Ok(entidad);
        }

        [HttpPut("{id}/cerrar")]
        public async Task<IActionResult> Cerrar(int id, [FromForm] VerificacionSncDto dto, IFormFile? evidenciaPdf)
        {
            var entidad = await _service.CerrarAsync(id, dto, evidenciaPdf);
            if (entidad == null) return NotFound();
            return Ok(entidad);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var eliminado = await _service.EliminarAsync(id, EsAdmin());
            if (!eliminado) return Forbid();
            return Ok();
        }

        [HttpPost("{id}/evidencias")]
        public async Task<IActionResult> SubirEvidencias(int id, [FromForm] string paso, [FromForm] List<IFormFile> fotos)
        {
            var evidencias = await _service.SubirEvidenciasAsync(id, paso, fotos);
            return Ok(evidencias);
        }

        [HttpGet("{id}/evidencias")]
        public async Task<IActionResult> GetEvidencias(int id)
        {
            var evidencias = await _service.ObtenerEvidenciasAsync(id);
            return Ok(evidencias);
        }

        [HttpGet("suma-filtrada")]
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
        public async Task<IActionResult> KgDelMes([FromQuery] int mes, [FromQuery] int anio)
        {
            var registros = await _service.ListarAsync(null, null, null, null, null, null, null);
            var total = _service.SumarKgDelMes(registros, mes, anio);
            return Ok(new { mes, anio, totalKg = total });
        }
    }
}