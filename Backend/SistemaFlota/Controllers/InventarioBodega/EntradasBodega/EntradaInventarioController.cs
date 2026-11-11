using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaFlota.DTOs.InventarioBodega.Inventario;
using SistemaFlota.Services.InventarioBodega.EntradasBodega;

namespace SistemaFlota.Controllers.InventarioBodega.EntradasBodega
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class EntradaInventarioController : ControllerBase
    {
        private readonly IEntradaInventarioService _service;

        public EntradaInventarioController(
            IEntradaInventarioService service)
        {
            _service = service;
        }

        // REGISTRAR ENTRADA DE PESO
        [HttpPost("ingresar-peso")]
        public async Task<IActionResult> IngresarPeso(
            [FromBody] IngresarPesoDto request)
        {
            try
            {
                var usuario = User.Identity?.Name ?? "Sistema";

                var resultado = await _service.IngresarPesoAsync(
                    request,
                    usuario);

                return Ok(resultado);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new
                {
                    message = ex.Message
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new
                {
                    message = ex.Message
                });
            }
            catch (Exception)
            {
                return StatusCode(500, new
                {
                    message = "Error al ingresar el peso."
                });
            }
        }
    }
}
