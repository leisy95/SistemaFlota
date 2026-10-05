using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaFlota.Authorization;
using SistemaFlota.DTOs.ComprasNoFormalizadas.OrdenesCompras;
using SistemaFlota.Services.ComprasNoFormalizadas.OrdenesCompras;

namespace SistemaFlota.Controllers.ComprasNoFormalizadas.OrdenesCompras
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class OrdenesCompraNoFormalizadasController : ControllerBase
    {
        private readonly IOrdenCompraNoFormalizadaService _service;
        private readonly IOrdenCompraNoFormalizadaPdfService _pdfService;

        public OrdenesCompraNoFormalizadasController(
            IOrdenCompraNoFormalizadaService service,
            IOrdenCompraNoFormalizadaPdfService pdfService)
        {
            _service = service;
            _pdfService = pdfService;
        }

        // Listar órdenes de compra
        [HttpGet]
        [Permiso("ordenes-compra-no-formalizadas", "ver")]
        [ProducesResponseType(
            typeof(OrdenCompraNoFormalizadaPaginadoDto),
            StatusCodes.Status200OK)]
        public async Task<ActionResult<OrdenCompraNoFormalizadaPaginadoDto>> Obtener(
            [FromQuery] string? search,
            [FromQuery] string? estado,
            [FromQuery] int? proveedorNoFormalizadoId,
            [FromQuery] string? formaPago,
            [FromQuery] DateTime? fechaInicio,
            [FromQuery] DateTime? fechaFin,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            var resultado = await _service.ObtenerAsync(
                search,
                estado,
                proveedorNoFormalizadoId,
                formaPago,
                fechaInicio,
                fechaFin,
                page,
                pageSize);

            return Ok(resultado);
        }

        // Obtener filtros dinámicos
        [HttpGet("filtros")]
        [Permiso("ordenes-compra-no-formalizadas", "ver")]
        [ProducesResponseType(
            typeof(FiltrosOrdenCompraNoFormalizadaDto),
            StatusCodes.Status200OK)]
        public async Task<ActionResult<FiltrosOrdenCompraNoFormalizadaDto>> ObtenerFiltros()
        {
            var filtros = await _service.ObtenerFiltrosAsync();

            return Ok(filtros);
        }

        // Para mostrar las órdenes de compra en recepción de mercancía
        [HttpGet("para-recepcion")]
        [Permiso("recepcion-mercancia-no-formalizada", "ver")]
        [ProducesResponseType(
            typeof(OrdenCompraNoFormalizadaPaginadoDto),
            StatusCodes.Status200OK)]
        public async Task<ActionResult<OrdenCompraNoFormalizadaPaginadoDto>> ObtenerParaRecepcion(
            [FromQuery] string? search,
            [FromQuery] string? estado,
            [FromQuery] int? proveedorNoFormalizadoId,
            [FromQuery] string? formaPago,
            [FromQuery] DateTime? fechaInicio,
            [FromQuery] DateTime? fechaFin,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            var resultado = await _service.ObtenerAsync(
                search,
                estado,
                proveedorNoFormalizadoId,
                formaPago,
                fechaInicio,
                fechaFin,
                page,
                pageSize);

            return Ok(resultado);
        }

        // Crear una nueva orden de compra
        [HttpPost]
        [Permiso("ordenes-compra-no-formalizadas", "crear")]
        [ProducesResponseType(
            typeof(OrdenCompraNoFormalizadaDto),
            StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<OrdenCompraNoFormalizadaDto>> Crear(
            [FromBody] CrearOrdenCompraNoFormalizadaDto dto)
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            var orden = await _service.CrearAsync(dto);

            return Ok(orden);
        }

        // Generar PDF de la orden de compra
        [HttpGet("{id:int}/pdf")]
        [Permiso("ordenes-compra-no-formalizadas", "ver")]
        public async Task<IActionResult> GenerarPdf(int id)
        {
            var pdf = await _pdfService.GenerarPdfAsync(id);

            return File(
                pdf,
                "application/pdf",
                $"OrdenCompraNoFormalizada-{id}.pdf");
        }

        // Obtener orden por id
        [HttpGet("{id:int}")]
        [Permiso("ordenes-compra-no-formalizadas", "ver")]
        [ProducesResponseType(
            typeof(OrdenCompraNoFormalizadaDto),
            StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<OrdenCompraNoFormalizadaDto>> ObtenerPorId(int id)
        {
            var orden = await _service.ObtenerPorIdAsync(id);

            if (orden == null)
                return NotFound();

            return Ok(orden);
        }

        // Actualizar una orden
        [HttpPut("{id:int}")]
        [Permiso("ordenes-compra-no-formalizadas", "editar")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> Actualizar(
            int id,
            [FromBody] ActualizarOrdenCompraNoFormalizadaDto dto)
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            var actualizado = await _service.ActualizarAsync(id, dto);

            if (!actualizado)
                return NotFound();

            return Ok();
        }

        // Enviar orden de compra por correo
        [HttpPost("{id:int}/enviar-correo")]
        [Permiso("ordenes-compra-no-formalizadas", "enviar-correo")]
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