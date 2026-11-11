
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaFlota.DTOs.InventarioBodega.Inventario;
using SistemaFlota.Services.InventarioBodega;

namespace SistemaFlota.Controllers.InventarioBodega.Inventario
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class InventarioBodegaController : ControllerBase
    {
        private readonly IInventarioBodegaService _service;

        public InventarioBodegaController(
            IInventarioBodegaService service)
        {
            _service = service;
        }

        // OBTENER TODOS
        [HttpGet]
        public async Task<IActionResult> Obtener()
        {
            try
            {
                var resultado = await _service.ObtenerAsync();
                return Ok(resultado);
            }
            catch (Exception)
            {
                return StatusCode(500, new
                {
                    message = "Error al consultar el inventario."
                });
            }
        }

        // OBTENER INVENTARIO PAGINADO
        [HttpGet("paginado")]
        public async Task<IActionResult> ObtenerPaginado(
            [FromQuery] string? buscar = null,
            [FromQuery] string? material = null,
            [FromQuery] string? color = null,
            [FromQuery] int pagina = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                var resultado = await _service.ObtenerPaginadoAsync(
                    buscar,
                    material,
                    color,
                    pagina,
                    pageSize
                );

                return Ok(resultado);
            }
            catch (Exception)
            {
                return StatusCode(500, new
                {
                    message = "Error al consultar el inventario paginado."
                });
            }
        }

        // BUSCAR POR REFERENCIA
        [HttpGet("referencia/{referencia}")]
        public async Task<IActionResult> ObtenerPorReferencia(
            string referencia)
        {
            try
            {
                var resultado =
                    await _service.ObtenerPorReferenciaAsync(referencia);

                if (resultado == null)
                {
                    return NotFound(new
                    {
                        message = "Referencia no encontrada."
                    });
                }

                return Ok(resultado);
            }
            catch (Exception)
            {
                return StatusCode(500, new
                {
                    message = "Error al consultar la referencia."
                });
            }
        }

        // REGISTRAR PRODUCTO
        [HttpPost("registrar")]
        public async Task<IActionResult> RegistrarItem(
            [FromBody] RegistrarItemDto request)
        {
            try
            {
                var resultado =
                    await _service.RegistrarItemAsync(request);

                return Ok(resultado);
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
                    message = "Error al registrar el producto."
                });
            }
        }

        // EDITAR INVENTARIO
        [HttpPut("editar/{id:int}")]
        public async Task<IActionResult> EditarInventario(
            int id,
            [FromBody] ActualizarInventarioBodegaDto request)
        {
            try
            {
                var resultado =
                    await _service.EditarInventarioAsync(id, request);

                if (!resultado)
                {
                    return NotFound(new
                    {
                        message = "Inventario no encontrado."
                    });
                }

                return Ok(new
                {
                    message = "Inventario actualizado correctamente."
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
                    message = "Error al actualizar el inventario."
                });
            }
        }

        // RECALCULAR INVENTARIO
        [HttpPost("recalcular")]
        public async Task<IActionResult> RecalcularInventario()
        {
            try
            {
                var resultado =
                    await _service.RecalcularInventarioAsync();

                return Ok(resultado);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new
                {
                    message = ex.Message
                });
            }
            catch (Exception)
            {
                return StatusCode(500, new
                {
                    message = "Error al recalcular el inventario."
                });
            }
        }

        // ELIMINAR INVENTARIO
        [HttpDelete("eliminar/{id:int}")]
        public async Task<IActionResult> EliminarInventario(int id)
        {
            try
            {
                var resultado =
                    await _service.EliminarInventarioAsync(id);

                if (!resultado)
                {
                    return NotFound(new
                    {
                        message = "Inventario no encontrado."
                    });
                }

                return Ok(new
                {
                    message = "Inventario eliminado correctamente."
                });
            }
            catch (DbUpdateException)
            {
                return BadRequest(new
                {
                    message = "No se puede eliminar el inventario porque tiene registros relacionados."
                });
            }
            catch (Exception)
            {
                return StatusCode(500, new
                {
                    message = "Error al eliminar el inventario."
                });
            }
        }
    }
}
