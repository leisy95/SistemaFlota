
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaFlota.Authorization;
using SistemaFlota.DTOs.InventarioBodega.Devoluciones;
using SistemaFlota.DTOs.InventarioBodega.EntradasBodega;
using SistemaFlota.DTOs.InventarioBodega.Inventario;
using SistemaFlota.Services.InventarioBodega.EntradasBodega;
using SistemaFlota.Services.InventarioBodega.EntradasBodega.EntradasInventarioPdf;

namespace SistemaFlota.Controllers.InventarioBodega.EntradasBodega
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class EntradaInventarioController : ControllerBase
    {
        private readonly IEntradaInventarioService _service;
        private readonly IEntradasInventarioPdfService _pdfService;

        public EntradaInventarioController(
            IEntradaInventarioService service,
            IEntradasInventarioPdfService pdfService)
        {
            _service = service;
            _pdfService = pdfService;
        }

        // OBTENER ENTRADAS DE INVENTARIO
        [HttpGet("mostrar-entradas")]
        [Permiso("entradas", "ver")]
        public async Task<IActionResult> ObtenerEntradas(
            [FromQuery] string? estado = null)
        {
            try
            {
                var resultado = await _service.ObtenerEntradasAsync(estado);

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

        // FILTRAR ENTRADAS CON PAGINACIÓN
        [HttpGet("filtrar-entradas")]
        [Permiso("entradas", "ver")]
        public async Task<IActionResult> FiltrarEntradas(
            [FromQuery] FiltroEntradaBodegaDto filtros)
        {
            try
            {
                var resultado = await _service.FiltrarEntradasAsync(filtros);

                return Ok(resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Error al filtrar las entradas de inventario.",
                    detalle = ex.Message
                });
            }
        }

        // EXPORTAR ENTRADAS A PDF
        [HttpGet("exportar-pdf")]
        [Permiso("entradas", "ver")]
        public async Task<IActionResult> ExportarPdf(
            [FromQuery] string? buscar = null,
            [FromQuery] DateTime? fechaInicio = null,
            [FromQuery] DateTime? fechaFin = null)
        {
            try
            {
                var pdf = await _pdfService.GenerarPdfAsync(
                    buscar,
                    fechaInicio,
                    fechaFin);

                var nombreArchivo =
                    $"Entradas_Inventario_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";

                return File(
                    pdf,
                    "application/pdf",
                    nombreArchivo);
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
                return NotFound(new
                {
                    message = ex.Message
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Error al generar el PDF de entradas de inventario.",
                    detalle = ex.Message
                });
            }
        }

        // REGISTRAR ENTRADA DE PESO
        [HttpPost("ingresar-peso")]
        [Permiso("registrar-entrada", "crear")]
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
