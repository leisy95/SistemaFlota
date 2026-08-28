using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaFlota.Authorization;
using SistemaFlota.DTOs.Costos.Inventario;
using SistemaFlota.Services.Costos.Inventario;

namespace SistemaFlota.Controllers.Costos.Inventario.AjustesInventario
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AjusteInventarioController : ControllerBase
    {
        private readonly IAjusteInventarioService _service;

        public AjusteInventarioController(
            IAjusteInventarioService service)
        {
            _service = service;
        }

        // Crear ajuste de inventario
        [HttpPost]
        [Permiso("inventario", "crear")]
        public async Task<ActionResult<AjusteInventarioDto>> Crear(
            CrearAjusteInventarioDto dto)
        {
            try
            {
                var ajuste = await _service.CrearAsync(dto);
                return Ok(ajuste);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // Consultar inventario para realizar ajuste
        [HttpGet("{inventarioId}")]
        [Permiso("inventario", "ver")]
        public async Task<ActionResult<InventarioAjusteDto>> Obtener(
            int inventarioId)
        {
            return Ok(await _service.ObtenerInventarioAsync(inventarioId));
        }

        // Historial de ajustes
        [HttpGet("historial/{inventarioId:int}")]
        [Permiso("inventario", "ver")]
        public async Task<ActionResult<List<AjusteInventarioDto>>> Historial(
            int inventarioId)
        {
            return Ok(await _service.ObtenerHistorialAsync(inventarioId));
        }
    }
}