using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaFlota.Authorization;
using SistemaFlota.DTOs.ComprasNoFormalizadas.Inventario.CortesInventario;
using SistemaFlota.Services.ComprasNoFormalizadas.Inventario.CortesInventario;

namespace SistemaFlota.Controllers.ComprasNoFormalizadas.Inventario.CortesInvenatrio
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CorteInventarioNoFormalizadoController : ControllerBase
    {
        private readonly ICorteInventarioNoFormalizadoService _service;
        private readonly ICorteInventarioNoFormalizadoPdfService _pdfService;

        public CorteInventarioNoFormalizadoController(
            ICorteInventarioNoFormalizadoService service,
            ICorteInventarioNoFormalizadoPdfService pdfService)
        {
            _service = service;
            _pdfService = pdfService;
        }

        // Consultar corte actual
        [HttpGet]
        [Permiso("inventario", "ver")]
        public async Task<ActionResult<List<CorteInventarioNoFormalizadoDto>>>
            ObtenerCorte()
        {
            var resultado = await _service.ObtenerCorteAsync();

            return Ok(resultado);
        }

        // Guardar corte
        [HttpPost]
        [Permiso("inventario", "editar")]
        public async Task<IActionResult> GuardarCorte(
            [FromBody] CrearCorteInventarioNoFormalizadoDto dto)
        {
            try
            {
                await _service.GuardarCorteAsync(dto);

                return Ok(new
                {
                    mensaje =
                        "Corte de inventario no formalizado guardado correctamente"
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new
                {
                    mensaje = ex.Message
                });
            }
            catch (Exception)
            {
                return StatusCode(500, new
                {
                    mensaje =
                        "Ocurrió un error al guardar el corte de inventario no formalizado."
                });
            }
        }

        // Historial
        [HttpGet("historial")]
        [Permiso("inventario", "ver")]
        public async Task<
            ActionResult<List<HistorialCorteInventarioNoFormalizadoDto>>>
            ObtenerHistorial()
        {
            var resultado = await _service.ObtenerHistorialAsync();

            return Ok(resultado);
        }

        // Detalle del corte
        [HttpGet("{id}")]
        [Permiso("inventario", "ver")]
        public async Task<
            ActionResult<HistorialCorteDetalleNoFormalizadoDto>>
            ObtenerDetalle(int id)
        {
            var resultado = await _service.ObtenerDetalleAsync(id);

            if (resultado == null)
                return NotFound();

            return Ok(resultado);
        }

        // Imprimir PDF
        [HttpGet("pdf")]
        [Permiso("inventario", "ver")]
        public async Task<IActionResult> GenerarPdf()
        {
            var pdf = await _pdfService.GenerarPdfAsync();

            return File(
                pdf,
                "application/pdf",
                $"CorteInventarioNoFormalizado-{DateTime.Now:yyyy-MM-dd}.pdf");
        }
    }
}