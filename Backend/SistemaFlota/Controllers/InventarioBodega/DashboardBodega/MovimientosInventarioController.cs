using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaFlota.Authorization;
using SistemaFlota.DTOs.InventarioBodega.DashboardBodega;
using SistemaFlota.Services.InventarioBodega.DashboardBodega;
using SistemaFlota.Services.InventarioBodega.DashboardBodega.MovimientosInventarioPdf;

namespace SistemaFlota.Controllers.InventarioBodega.DashboardBodega
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class MovimientosInventarioController : ControllerBase
    {
        private readonly IMovimientosInventarioService _service;
        private readonly IMovimientosInventarioPdfService _pdfService;

        public MovimientosInventarioController(
            IMovimientosInventarioService service,
            IMovimientosInventarioPdfService pdfService)
        {
            _service = service;
            _pdfService = pdfService;
        }

        // LISTAR MOVIMIENTOS
        [HttpGet]
        [Permiso("historial-movimientos-bodega", "ver")]
        public async Task<ActionResult<IEnumerable<MovimientoInventarioResponseDto>>>
            GetMovimientos()
        {
            var movimientos = await _service.ObtenerMovimientosAsync();

            return Ok(movimientos);
        }

        // OBTENER MOVIMIENTO POR ID
        [HttpGet("{id}")]
        [Permiso("historial-movimientos-bodega", "ver")]
        public async Task<ActionResult<MovimientoInventarioResponseDto>>
            GetMovimiento(int id)
        {
            var movimiento = await _service.ObtenerMovimientoPorIdAsync(id);

            if (movimiento == null)
                return NotFound();

            return Ok(movimiento);
        }

        // CONSULTAR AUDITORÍA
        [HttpGet("auditoria")]
        [Permiso("historial-movimientos-bodega", "ver")]
        public async Task<ActionResult<MovimientoInventarioAuditoriaCompletaDto>>
            GetAuditoria([FromQuery] MovimientoInventarioFiltroDto filtros)
        {
            var auditoria = await _service.ObtenerAuditoriaAsync(filtros);

            return Ok(auditoria);
        }

        // GENERAR PDF DE MOVIMIENTOS FILTRADOS
        [HttpPost("pdf")]
        [Permiso("historial-movimientos-bodega", "ver")]
        public async Task<IActionResult> GenerarPdf(
            [FromBody] MovimientoInventarioFiltroDto filtros)
        {
            try
            {
                var pdf = await _pdfService.GenerarPdfAsync(filtros);

                return File(
                    pdf,
                    "application/pdf",
                    "movimientos-inventario.pdf"
                );
            }
            catch (Exception ex)
            {
                return StatusCode(
                    500,
                    new
                    {
                        mensaje = "Error al generar el PDF de movimientos.",
                        detalle = ex.Message
                    }
                );
            }
        }

        //FILTROS 
        [HttpGet("filtros/opciones")]
        [Permiso("historial-movimientos-bodega", "ver")]
        public async Task<IActionResult> ObtenerOpcionesFiltros()
        {
            var respuesta = await _service.ObtenerOpcionesFiltrosAsync();
            return Ok(respuesta);
        }

        // REGISTRAR MOVIMIENTO
        [HttpPost]
        [Permiso("historial-movimientos-bodega", "crear")]
        public async Task<ActionResult<MovimientoInventarioResponseDto>>
            PostMovimiento(MovimientoInventarioRequestDto request)
        {
            var movimiento = await _service.RegistrarMovimientoAsync(request);

            return CreatedAtAction(
                nameof(GetMovimiento),
                new { id = movimiento.Id },
                movimiento);
        }
    }
}
