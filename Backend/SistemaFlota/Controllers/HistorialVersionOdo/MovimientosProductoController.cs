using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaFlota.DTOs.HistorialVersionOdo;
using SistemaFlota.Services.HistorialVersionOdo;

namespace SistemaFlota.Controllers.HistorialVersionOdo
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class MovimientosProductoController : ControllerBase
    {
        private readonly IMovimientoProductoService _movimientoProductoService;

        public MovimientosProductoController(
            IMovimientoProductoService movimientoProductoService)
        {
            _movimientoProductoService = movimientoProductoService;
        }

        [HttpGet]
        public async Task<ActionResult<PaginacionDto<MovimientoProductoDto>>> Get(
            [FromQuery] int pagina = 1,
            [FromQuery] int porPagina = 50,
            [FromQuery] string? buscar = null,
            [FromQuery] string? producto = null,
            [FromQuery] string? proveedor = null,
            [FromQuery] string? estado = null,
            [FromQuery] string? unidadMedida = null,
            [FromQuery] DateTime? fechaDesde = null,
            [FromQuery] DateTime? fechaHasta = null)
        {
            var resultado = await _movimientoProductoService.ObtenerAsync(
                pagina,
                porPagina,
                buscar,
                producto,
                proveedor,
                estado,
                unidadMedida,
                fechaDesde,
                fechaHasta);

            return Ok(resultado);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<MovimientoProductoDto>> GetById(int id)
        {
            var movimiento =
                await _movimientoProductoService.ObtenerPorIdAsync(id);

            if (movimiento == null)
            {
                return NotFound(new
                {
                    mensaje = "Movimiento de producto no encontrado."
                });
            }

            return Ok(movimiento);
        }
    }
}
