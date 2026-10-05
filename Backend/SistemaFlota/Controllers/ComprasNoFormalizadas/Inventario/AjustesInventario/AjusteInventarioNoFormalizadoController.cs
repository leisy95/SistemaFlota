using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaFlota.Authorization;
using SistemaFlota.DTOs.ComprasNoFormalizadas.Inventario;
using SistemaFlota.Services.ComprasNoFormalizadas.Inventario.AjustesInventarios;

namespace SistemaFlota.Controllers.ComprasNoFormalizadas.Inventario.AjustesInventario
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AjusteInventarioNoFormalizadoController : ControllerBase
    {
        private readonly IAjusteInventarioNoFormalizadoService _service;

        public AjusteInventarioNoFormalizadoController(
            IAjusteInventarioNoFormalizadoService service)
        {
            _service = service;
        }

        // Crear ajuste de inventario
        [HttpPost]
        [Permiso("inventario", "crear")]
        public async Task<ActionResult<AjusteInventarioNoFormalizadoDto>> Crear(
            CrearAjusteInventarioNoFormalizadoDto dto)
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
        public async Task<ActionResult<InventarioAjusteNoFormalizadoDto>> Obtener(
            int inventarioId)
        {
            return Ok(
                await _service.ObtenerInventarioAsync(inventarioId));
        }

        // Historial de ajustes
        [HttpGet("historial/{inventarioId:int}")]
        [Permiso("inventario", "ver")]
        public async Task<
            ActionResult<List<AjusteInventarioNoFormalizadoDto>>>
            Historial(int inventarioId)
        {
            return Ok(
                await _service.ObtenerHistorialAsync(inventarioId));
        }
    }
}