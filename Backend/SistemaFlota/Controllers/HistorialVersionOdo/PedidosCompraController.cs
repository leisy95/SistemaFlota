using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaFlota.Authorization;
using SistemaFlota.DTOs.HistorialVersionOdo;
using SistemaFlota.Services.HistorialVersionOdo;

namespace SistemaFlota.Controllers.HistorialVersionOdo
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PedidosCompraController : ControllerBase
    {
        private readonly IPedidoCompraService _pedidoCompraService;

        public PedidosCompraController(IPedidoCompraService pedidoCompraService)
        {
            _pedidoCompraService = pedidoCompraService;
        }

        [HttpGet]
        [Permiso("historial-odo-pedido-compra", "ver")]
        public async Task<ActionResult<PaginacionDto<PedidoCompraDto>>> Get(
            [FromQuery] int pagina = 1,
            [FromQuery] int porPagina = 20,
            [FromQuery] string? buscar = null,
            [FromQuery] string? prioridad = null,
            [FromQuery] string? estado = null,
            [FromQuery] DateTime? fechaDesde = null,
            [FromQuery] DateTime? fechaHasta = null)
        {
            var resultado = await _pedidoCompraService.ObtenerAsync(
                pagina, porPagina, buscar, prioridad, estado, fechaDesde, fechaHasta);

            return Ok(resultado);
        }

        [HttpGet("estadisticas")]
        [Permiso("historial-odo-pedido-compra", "ver")]
        public async Task<ActionResult<EstadisticasPedidoCompraDto>> GetEstadisticas(
            [FromQuery] string? buscar = null,
            [FromQuery] string? prioridad = null,
            [FromQuery] string? estado = null,
            [FromQuery] DateTime? fechaDesde = null,
            [FromQuery] DateTime? fechaHasta = null)
        {
            var resultado = await _pedidoCompraService.ObtenerEstadisticasAsync(
                buscar, prioridad, estado, fechaDesde, fechaHasta);

            return Ok(resultado);
        }

        [HttpGet("{id}")]
        [Permiso("historial-odo-pedido-compra", "ver")]
        public async Task<ActionResult<PedidoCompraDto>> GetById(int id)
        {
            var pedido = await _pedidoCompraService.ObtenerPorIdAsync(id);

            if (pedido == null)
            {
                return NotFound(new
                {
                    mensaje = "Pedido de compra no encontrado."
                });
            }

            return Ok(pedido);
        }
    }
}