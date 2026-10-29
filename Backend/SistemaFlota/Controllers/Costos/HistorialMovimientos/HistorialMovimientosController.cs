using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaFlota.DTOs.Costos.HistorialMovimientos;
using SistemaFlota.Services.Costos.HistorialMovimientos;

namespace SistemaFlota.Controllers.Costos.HistorialMovimientos
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class HistorialMovimientosController : ControllerBase
    {
        private readonly IHistorialMovimientosService _service;

        public HistorialMovimientosController(
            IHistorialMovimientosService service)
        {
            _service = service;
        }

        [HttpGet]
        [ProducesResponseType(
            typeof(MovimientoHistorialPaginadoDto),
            StatusCodes.Status200OK)]
        public async Task<ActionResult<MovimientoHistorialPaginadoDto>> Obtener(
            [FromQuery] DateTime? desde,
            [FromQuery] DateTime? hasta,
            [FromQuery] string? modulo,
            [FromQuery] string? accion,
            [FromQuery] string? usuario,
            [FromQuery] string? buscar,
            [FromQuery] int pagina = 1,
            [FromQuery] int porPagina = 10)
        {
            if (desde.HasValue && hasta.HasValue &&
                desde.Value.Date > hasta.Value.Date)
            {
                return BadRequest(new
                {
                    mensaje = "La fecha inicial no puede ser posterior a la fecha final."
                });
            }

            var resultado = await _service.ObtenerAsync(
                desde, hasta, modulo, accion, usuario,
                buscar, pagina, porPagina);

            return Ok(resultado);
        }

        [HttpGet("resumen")]
        [ProducesResponseType(
            typeof(MovimientoHistorialResumenDto),
            StatusCodes.Status200OK)]
        public async Task<ActionResult<MovimientoHistorialResumenDto>>
            ObtenerResumen(
                [FromQuery] DateTime? desde,
                [FromQuery] DateTime? hasta,
                [FromQuery] string? modulo,
                [FromQuery] string? accion,
                [FromQuery] string? usuario,
                [FromQuery] string? buscar)
        {
            if (desde.HasValue && hasta.HasValue &&
                desde.Value.Date > hasta.Value.Date)
            {
                return BadRequest(new
                {
                    mensaje = "La fecha inicial no puede ser posterior a la fecha final."
                });
            }

            var resultado = await _service.ObtenerResumenAsync(
                desde, hasta, modulo, accion, usuario, buscar);

            return Ok(resultado);
        }

        [HttpGet("{id:int}")]
        [ProducesResponseType(
            typeof(MovimientoHistorialDto),
            StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<MovimientoHistorialDto>> ObtenerPorId(
            int id)
        {
            var movimiento = await _service.ObtenerPorIdAsync(id);

            if (movimiento == null)
                return NotFound();

            return Ok(movimiento);
        }

        [HttpGet("opciones")]
        public async Task<ActionResult<MovimientoHistorialOpcionesDto>> ObtenerOpciones()
        {
            var opciones = await _service.ObtenerOpcionesAsync();
            return Ok(opciones);
        }
    }
}