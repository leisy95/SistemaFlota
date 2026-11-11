using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaFlota.DTOs.InventarioBodega.SalidasBodega;
using SistemaFlota.Services.InventarioBodega.SalidasBodega;

namespace SistemaFlota.Controllers.InventarioBodega.SalidasBodega
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class SalidasBodegaController : ControllerBase
    {
        private readonly ISalidaBodegaService _salidaBodegaService;

        public SalidasBodegaController(
            ISalidaBodegaService salidaBodegaService)
        {
            _salidaBodegaService = salidaBodegaService;
        }

        // OBTENER SALIDA ACTUAL
        [HttpGet("actual")]
        public async Task<IActionResult> ObtenerSalidaActual()
        {
            var respuesta =
                await _salidaBodegaService.ObtenerSalidaActualAsync();

            return Ok(respuesta);
        }

        // CREAR SALIDA
        [HttpPost("crear")]
        public async Task<IActionResult> CrearSalida()
        {
            var respuesta =
                await _salidaBodegaService.CrearSalidaAsync();

            return Ok(respuesta);
        }

        // AGREGAR ITEM POR ID
        [HttpPost("{idSalida:int}/items/{idInventarioBodegaItem:int}")]
        public async Task<IActionResult> AgregarItem(
            int idSalida,
            int idInventarioBodegaItem)
        {
            var respuesta =
                await _salidaBodegaService.AgregarItemAsync(
                    idSalida,
                    idInventarioBodegaItem
                );

            return Ok(respuesta);
        }

        // AGREGAR ITEM POR REFERENCIA
        [HttpPost("{idSalida:int}/agregar-por-referencia")]
        public async Task<IActionResult> AgregarItemPorReferencia(
            int idSalida,
            [FromBody] string referencia)
        {
            var respuesta =
                await _salidaBodegaService.AgregarItemPorReferenciaAsync(
                    idSalida,
                    referencia
                );

            return Ok(respuesta);
        }

        // QUITAR ITEM
        [HttpDelete("{idSalida:int}/items/{idInventarioBodegaItem:int}")]
        public async Task<IActionResult> QuitarItem(
            int idSalida,
            int idInventarioBodegaItem)
        {
            var respuesta =
                await _salidaBodegaService.QuitarItemAsync(
                    idSalida,
                    idInventarioBodegaItem
                );

            return Ok(respuesta);
        }

        // CONFIRMAR SALIDA
        [HttpPost("{idSalida:int}/confirmar")]
        public async Task<IActionResult> ConfirmarSalida(
            int idSalida,
            [FromBody] ConfirmarSalidaBodegaDto dto)
        {
            var respuesta =
                await _salidaBodegaService.ConfirmarSalidaAsync(
                    idSalida,
                    dto
                );

            return Ok(respuesta);
        }

        // CANCELAR SALIDA
        [HttpPost("{idSalida:int}/cancelar")]
        public async Task<IActionResult> CancelarSalida(int idSalida)
        {
            var respuesta =
                await _salidaBodegaService.CancelarSalidaAsync(idSalida);

            return Ok(respuesta);
        }
    }
}
