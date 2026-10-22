
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaFlota.DTOs.ControlEnvios.CostosFletes;
using SistemaFlota.Services.ControlEnvios.CostosFlete;
using SistemaFlota.Services.ControlEnvios.CostosFlete.CostosFletePdf;
using SistemaFlota.Services.ControlEnvios.CostosFletesPdf;
using System.Security.Claims;

namespace SistemaFlota
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CostosFletesController : ControllerBase
    {
        private readonly ICostosFletesService _service;
        private readonly ICostosFletesPdfService _pdfService;

        public CostosFletesController(
            ICostosFletesService service,
            ICostosFletesPdfService pdfService)
        {
            _service = service;
            _pdfService = pdfService;
        }

        private string GetUsuario() =>
            User.FindFirstValue(ClaimTypes.Name) ?? "";

        private string GetRol() =>
            User.FindFirstValue(ClaimTypes.Role) ?? "";

        // GET: api/CostosFletes
        [HttpGet]
        public async Task<IActionResult> Get(
            [FromQuery] string? desde = null,
            [FromQuery] string? hasta = null,
            [FromQuery] string? conductor = null,
            [FromQuery] string? estado = null,
            [FromQuery] string? ciudad = null,
            [FromQuery] string? placa = null)
        {
            var lista = await _service.ObtenerAsync(
                desde,
                hasta,
                conductor,
                estado,
                ciudad,
                placa);

            return Ok(lista);
        }

        // GET: api/CostosFletes/pdf
        [HttpGet("pdf")]
        public async Task<IActionResult> GenerarPdf(
            [FromQuery] string? desde = null,
            [FromQuery] string? hasta = null,
            [FromQuery] string? conductor = null,
            [FromQuery] string? estado = null,
            [FromQuery] string? ciudad = null,
            [FromQuery] string? placa = null)
        {
            var pdf = await _pdfService.GenerarPdfAsync(
                desde,
                hasta,
                conductor,
                estado,
                ciudad,
                placa);

            var nombreArchivo =
                $"Reporte_Costos_Fletes_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";

            return File(
                pdf,
                "application/pdf",
                nombreArchivo);
        }

        // POST: api/CostosFletes
        [HttpPost]
        public async Task<IActionResult> Crear(
            [FromBody] CostoFleteDto dto)
        {
            var registro = await _service.CrearAsync(
                dto,
                GetUsuario(),
                GetRol());

            return Ok(registro);
        }

        // PUT: api/CostosFletes/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Editar(
            int id,
            [FromBody] CostoFleteDto dto)
        {
            var registro = await _service.EditarAsync(id, dto);

            if (registro == null)
                return NotFound();

            return Ok(registro);
        }

        // PUT: api/CostosFletes/{id}/verificar
        [HttpPut("{id}/verificar")]
        public async Task<IActionResult> Verificar(
            int id,
            [FromBody] VerificarDto dto)
        {
            var registro = await _service.VerificarAsync(
                id,
                dto,
                GetUsuario(),
                GetRol());

            if (registro == null)
                return NotFound();

            return Ok(registro);
        }

        // DELETE: api/CostosFletes/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Eliminar(int id)
        {
            var eliminado = await _service.EliminarAsync(id);

            if (!eliminado)
                return NotFound();

            return Ok();
        }
    }
}
