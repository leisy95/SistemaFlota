using Microsoft.AspNetCore.Mvc;
using SistemaFlota.Authorization;
using SistemaFlota.DTOs.Costos.OrdenesTraslado;
using SistemaFlota.Services.Costos.OrdenesTraslado;

namespace SistemaFlota.Controllers.Costos.OrdenesTraslado
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrdenTrasladoController : ControllerBase
    {
        private readonly IOrdenTrasladoService _service;

        public OrdenTrasladoController(IOrdenTrasladoService service)
        {
            _service = service;
        }


        // crear Orden de traslado
        [HttpPost]
        [Permiso("traslados", "crear")]
        public async Task<IActionResult> Crear([FromBody] CrearOrdenTrasladoDto dto)
        {
            try
            {
                var resultado = await _service.CrearAsync(dto);
                return Ok(resultado);
            }
            catch (Exception ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }

        // Obtener traslado por ID
        [HttpGet("{id}")]
        [Permiso("traslados", "ver")]
        public async Task<IActionResult> Obtener(int id)
        {
            var resultado = await _service.ObtenerPorIdAsync(id);

            if (resultado == null)
                return NotFound(new { mensaje = "Orden de traslado no encontrada." });

            return Ok(resultado);
        }

        //Listar todos los tralados
        [HttpGet]
        [Permiso("traslados", "ver")]
        public async Task<IActionResult> ObtenerTodos(
            [FromQuery] string? search,
            [FromQuery] string? estado,
            [FromQuery] string? destino,
            [FromQuery] DateTime? fechaInicio,
            [FromQuery] DateTime? fechaFin,
            [FromQuery] int pagina = 1,
            [FromQuery] int tamanoPagina = 10)
        {
            var resultado = await _service.ObtenerTodosAsync(
                search,
                estado,
                destino,
                fechaInicio,
                fechaFin,
                pagina,
                tamanoPagina);

            return Ok(resultado);
        }

        // Verificar orden
        [HttpPut("verificar")]
        [Permiso("traslados", "verificar")]
        public async Task<IActionResult> Verificar(
            [FromBody] VerificarOrdenTrasladoDto dto)
        {
            try
            {
                var resultado = await _service.VerificarAsync(dto);

                return Ok(resultado);
            }
            catch (Exception ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }

        // Confirmar orden
        [HttpPut("{id}/confirmar")]
        [Permiso("traslados", "confirmar")]
        public async Task<IActionResult> Confirmar(int id)
        {
            try
            {
                var resultado = await _service.ConfirmarAsync(id);

                return Ok(resultado);
            }
            catch (Exception ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }
    }
}