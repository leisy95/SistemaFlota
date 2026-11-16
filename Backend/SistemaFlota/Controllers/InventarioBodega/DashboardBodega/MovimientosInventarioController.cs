using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaFlota.DTOs.InventarioBodega.DashboardBodega;
using SistemaFlota.Services.InventarioBodega.DashboardBodega;

namespace SistemaFlota.Controllers.InventarioBodega.DashboardBodega
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class MovimientosInventarioController : ControllerBase
    {
        private readonly IMovimientosInventarioService _service;

        public MovimientosInventarioController(
            IMovimientosInventarioService service)
        {
            _service = service;
        }

        // GET: api/MovimientosInventario
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MovimientoInventarioResponseDto>>>
            GetMovimientos()
        {
            var movimientos = await _service.ObtenerMovimientosAsync();

            return Ok(movimientos);
        }

        // GET: api/MovimientosInventario/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<MovimientoInventarioResponseDto>>
            GetMovimiento(int id)
        {
            var movimiento = await _service.ObtenerMovimientoPorIdAsync(id);

            if (movimiento == null)
                return NotFound();

            return Ok(movimiento);
        }

        // GET: api/MovimientosInventario/auditoria
        [HttpGet("auditoria")]
        public async Task<ActionResult<MovimientoInventarioAuditoriaCompletaDto>>
            GetAuditoria([FromQuery] MovimientoInventarioFiltroDto filtros)
        {
            var auditoria = await _service.ObtenerAuditoriaAsync(filtros);

            return Ok(auditoria);
        }

        // POST: api/MovimientosInventario
        [HttpPost]
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
