using Microsoft.AspNetCore.Mvc;
using SistemaFlota.Authorization;
using SistemaFlota.DTOs.ComprasNoFormalizadas.OrdenesTraslado;
using SistemaFlota.Services.ComprasNoFormalizadas.OrdenesTraslado;

namespace SistemaFlota.Controllers.ComprasNoFormalizadas.OrdenesTraslado
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrdenTrasladoNoFormalizadaController : ControllerBase
    {
        private readonly IOrdenTrasladoNoFormalizadaService _service;

        public OrdenTrasladoNoFormalizadaController(
            IOrdenTrasladoNoFormalizadaService service)
        {
            _service = service;
        }

        // Crear orden de traslado
        [HttpPost]
        [Permiso("traslados", "crear")]
        public async Task<IActionResult> Crear(
            [FromBody] CrearOrdenTrasladoNoFormalizadaDto dto)
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
                return NotFound(new
                {
                    mensaje = "Orden de traslado no formalizada no encontrada."
                });

            return Ok(resultado);
        }

        // Listar todos los traslados
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
        [Permiso("traslados", "editar")]
        public async Task<IActionResult> Verificar(
            [FromBody] VerificarOrdenTrasladoNoFormalizadaDto dto)
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
        [Permiso("traslados", "editar")]
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
