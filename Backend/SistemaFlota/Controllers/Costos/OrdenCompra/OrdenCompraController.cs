using Microsoft.AspNetCore.Mvc;
using SistemaFlota.DTOs.Costos.OrdenCompra;
using SistemaFlota.Services.Costos.OrdenCompra;

namespace SistemaFlota.Controllers.Costos.OrdenCompra
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrdenCompraController : ControllerBase
    {
        private readonly IOrdenCompraService _service;
        private readonly IOrdenCompraPdfService _pdfService;

        public OrdenCompraController(
            IOrdenCompraService service,
            IOrdenCompraPdfService pdfService
            )
        {
            _service = service;
            _pdfService = pdfService;
        }

        // Listar órdenes de compra
        [HttpGet]
        [ProducesResponseType(typeof(OrdenCompraPaginadoDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<OrdenCompraPaginadoDto>> Obtener(
            [FromQuery] string? search,
            [FromQuery] string? estado,
            [FromQuery] int? proveedorId,
            [FromQuery] string? formaPago,
            [FromQuery] DateTime? fechaInicio,
            [FromQuery] DateTime? fechaFin,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            var resultado = await _service.ObtenerAsync(
                search,
                estado,
                proveedorId,
                formaPago,
                fechaInicio,
                fechaFin,
                page,
                pageSize);

            return Ok(resultado);
        }


        // Crear una nueva orden de compra.
        [HttpPost]
        [ProducesResponseType(typeof(OrdenCompraDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<OrdenCompraDto>> Crear(
            [FromBody] CrearOrdenCompraDto dto)
        {
            var orden = await _service.CrearAsync(dto);

            return Ok(orden);
        }

        // Para generar pdf orden compra
        [HttpGet("{id}/pdf")]
        public async Task<IActionResult> GenerarPdf(int id)
        {
            var pdf = await _pdfService.GenerarPdfAsync(id);

            return File(
                pdf,
                "application/pdf",
                $"OrdenCompra-{id}.pdf");
        }

        // Para editar una orden por id
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(OrdenCompraDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<OrdenCompraDto>> ObtenerPorId(int id)
        {
            var orden = await _service.ObtenerPorIdAsync(id);

            if (orden == null)
                return NotFound();

            return Ok(orden);
        }

        // Para actualizar una orden
        [HttpPut("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> Actualizar(
            int id,
            [FromBody] ActualizarOrdenCompraDto dto)
        {
            var actualizado = await _service.ActualizarAsync(id, dto);

            if (!actualizado)
                return NotFound();

            return Ok();
        }

        // Enviar orden de compra por correo

        [HttpPost("{id:int}/enviar-correo")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> EnviarCorreo(int id)
        {
            try
            {
                await _service.EnviarPorCorreoAsync(id);

                return Ok(new
                {
                    mensaje = "La orden de compra fue enviada correctamente por correo."
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    mensaje = ex.Message
                });
            }
        }
    }
}
