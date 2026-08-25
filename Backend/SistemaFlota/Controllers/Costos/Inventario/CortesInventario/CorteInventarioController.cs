using Microsoft.AspNetCore.Mvc;
using SistemaFlota.DTOs.Costos.Inventario.CortesInventario;
using SistemaFlota.Services.Costos.Inventario.CortesInventario;

namespace SistemaFlota.Controllers.Costos.Inventario.CortesInventario
{
    [ApiController]
    [Route("api/[controller]")]
    public class CorteInventarioController : ControllerBase
    {
        private readonly ICorteInventarioService _service;
        private readonly ICorteInventarioPdfService _pdfService;
        public CorteInventarioController(
            ICorteInventarioService service,
            ICorteInventarioPdfService pdfService
            )
        {
            _service = service;
            _pdfService = pdfService;
        }

        [HttpGet]
        public async Task<ActionResult<List<CorteInventarioDto>>> ObtenerCorte()
        {
            var resultado = await _service.ObtenerCorteAsync();

            return Ok(resultado);
        }

        [HttpPost]
        public async Task<IActionResult> GuardarCorte([FromBody] CrearCorteInventarioDto dto)
        {
            try
            {
                await _service.GuardarCorteAsync(dto);
                return Ok(new { mensaje = "Corte de inventario guardado correctamente" });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, new { mensaje = "Ocurrió un error al guardar el corte de inventario." });
            }
        }

        [HttpGet("historial")]
        public async Task<ActionResult<List<HistorialCorteInventarioDto>>> ObtenerHistorial()
        {
            var resultado = await _service.ObtenerHistorialAsync();
            return Ok(resultado);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<HistorialCorteDetalleDto>> ObtenerDetalle(int id)
        {
            var resultado = await _service.ObtenerDetalleAsync(id);

            if (resultado == null)
                return NotFound();

            return Ok(resultado);
        }

        // Imprimir pdf
        [HttpGet("pdf")]
        public async Task<IActionResult> GenerarPdf()
        {
            var pdf = await _pdfService.GenerarPdfAsync();

            return File(
                pdf,
                "application/pdf",
                $"CorteInventario-{DateTime.Now:yyyy-MM-dd}.pdf");
        }
    }
}
