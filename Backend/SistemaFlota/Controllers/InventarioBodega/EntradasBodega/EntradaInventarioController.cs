
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaFlota.DTOs.InventarioBodega.Devoluciones;
using SistemaFlota.DTOs.InventarioBodega.EntradasBodega;
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

        // OBTENER ENTRADAS DE INVENTARIO
        [HttpGet("mostrar-entradas")]
        public async Task<IActionResult> ObtenerEntradas()
        {
            try
            {
                var resultado = await _service.ObtenerEntradasAsync();

                return Ok(resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Error al consultar las entradas de inventario.",
                    detalle = ex.Message
                });
            }
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

        // REGISTRAR DEVOLUCIÓN
        [HttpPost("registrar-devolucion")]
        public async Task<IActionResult> RegistrarDevolucion(
            [FromBody] DevolucionEntradaDto request)
        {
            try
            {
                var usuario = User.Identity?.Name ?? "Sistema";

                var resultado = await _service.RegistrarDevolucionAsync(
                    request,
                    usuario);

                return Ok(new
                {
                    message = resultado
                });
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
            catch (InvalidOperationException ex)
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
                    message = "Error al registrar la devolución."
                });
            }
        }

        // AJUSTE GENERAL DE INVENTARIO
        [HttpPost("ajuste-general")]
        public async Task<IActionResult> AjusteGeneral(
            [FromBody] AjusteGeneralEntradaDto request)
        {
            try
            {
                var usuario = User.Identity?.Name ?? "Sistema";

                var resultado = await _service.AjusteGeneralAsync(
                    request,
                    usuario);

                return Ok(new
                {
                    message = resultado
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new
                {
                    message = ex.Message
                });
            }
            catch (InvalidOperationException ex)
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
                    message = "Error al realizar el ajuste general."
                });
            }
        }
    }
}
