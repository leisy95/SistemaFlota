using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaFlota.Authorization;
using SistemaFlota.Services.InventarioBodega.HistorialMovimientos;

namespace SistemaFlota.Controllers.InventarioBodega.HistorialMovimientos
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class HistorialInventarioBodegaController : ControllerBase
    {
        private readonly IHistorialInventarioBodegaService _service;

        public HistorialInventarioBodegaController(
            IHistorialInventarioBodegaService service)
        {
            _service = service;
        }

        // CONSULTAR HISTORIAL
        [HttpGet]
        [Permiso("historial-movimientos-bodega", "ver")]
        public async Task<IActionResult> Obtener(
            [FromQuery] DateTime? desde,
            [FromQuery] DateTime? hasta,
            [FromQuery] string? modulo,
            [FromQuery] string? accion,
            [FromQuery] string? usuario,
            [FromQuery] string? buscar,
            [FromQuery] int pagina = 1,
            [FromQuery] int porPagina = 10)
        {
            var resultado = await _service.ObtenerAsync(
                desde, hasta, modulo, accion,
                usuario, buscar, pagina, porPagina);

            return Ok(resultado);
        }

        // CONSULTAR RESUMEN
        [HttpGet("resumen")]
        [Permiso("historial-movimientos-bodega", "ver")]
        public async Task<IActionResult> ObtenerResumen(
            [FromQuery] DateTime? desde,
            [FromQuery] DateTime? hasta,
            [FromQuery] string? modulo,
            [FromQuery] string? accion,
            [FromQuery] string? usuario,
            [FromQuery] string? buscar)
        {
            var resultado = await _service.ObtenerResumenAsync(
                desde, hasta, modulo, accion, usuario, buscar);

            return Ok(resultado);
        }

        // OBTENER OPCIONES DE FILTROS
        [HttpGet("opciones")]
        [Permiso("historial-movimientos-bodega", "ver")]
        public async Task<IActionResult> ObtenerOpciones()
        {
            var resultado = await _service.ObtenerOpcionesAsync();
            return Ok(resultado);
        }

        // CONSULTAR MOVIMIENTO POR ID
        [HttpGet("{id:int}")]
        [Permiso("historial-movimientos-bodega", "ver")]
        public async Task<IActionResult> ObtenerPorId(int id)
        {
            var resultado = await _service.ObtenerPorIdAsync(id);

            if (resultado == null)
                return NotFound(new { mensaje = "Movimiento no encontrado." });

            return Ok(resultado);
        }
    }
}
