using Microsoft.AspNetCore.Mvc;
using SistemaFlota.Authorization;
using SistemaFlota.DTOs.ComprasNoFormalizadas.OrdenesCompras;
using SistemaFlota.Services.ComprasNoFormalizadas.OrdenesCompras;

namespace SistemaFlota.Controllers.ComprasNoFormalizadas.OrdenesCompras
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrdenesCompraNoFormalizadasController : ControllerBase
    {
        private readonly IOrdenCompraNoFormalizadaService _ordenCompraService;

        public OrdenesCompraNoFormalizadasController(IOrdenCompraNoFormalizadaService ordenCompraService)
        {
            _ordenCompraService = ordenCompraService;
        }

        [HttpGet]
        [Permiso("ordenes-compra-no-formalizadas", "ver")]
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
            var resultado = await _ordenCompraService.ObtenerAsync(
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

        [HttpGet("{id:int}")]
        [Permiso("ordenes-compra-no-formalizadas", "ver")]
        public async Task<ActionResult<OrdenCompraNoFormalizadaDto>> ObtenerPorId(int id)
        {
            var orden = await _ordenCompraService.ObtenerPorIdAsync(id);

            if (orden == null)
                return NotFound();

            return Ok(orden);
        }

        [HttpGet("filtros")]
        [Permiso("ordenes-compra-no-formalizadas", "ver")]
        public async Task<ActionResult<FiltrosOrdenCompraNoFormalizadaDto>> ObtenerFiltros()
        {
            var filtros = await _ordenCompraService.ObtenerFiltrosAsync();
            return Ok(filtros);
        }

        [HttpPost]
        [Permiso("ordenes-compra-no-formalizadas", "crear")]
        public async Task<ActionResult<OrdenCompraNoFormalizadaDto>> Crear(
            [FromBody] CrearOrdenCompraNoFormalizadaDto dto)
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            var orden = await _ordenCompraService.CrearAsync(dto);

            return CreatedAtAction(
                nameof(ObtenerPorId),
                new { id = orden.Id },
                orden);
        }

        [HttpPut("{id:int}")]
        [Permiso("ordenes-compra-no-formalizadas", "editar")]
        public async Task<IActionResult> Actualizar(
            int id,
            [FromBody] ActualizarOrdenCompraNoFormalizadaDto dto)
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            var actualizado = await _ordenCompraService.ActualizarAsync(id, dto);

            if (!actualizado)
                return NotFound();

            return NoContent();
        }

        [HttpPost("{id:int}/enviar-correo")]
        [Permiso("ordenes-compra-no-formalizadas", "editar")]
        public async Task<IActionResult> EnviarPorCorreo(int id)
        {
            await _ordenCompraService.EnviarPorCorreoAsync(id);
            return Ok(new { mensaje = "La orden de compra fue enviada correctamente." });
        }

        [HttpDelete("{id:int}")]
        [Permiso("ordenes-compra-no-formalizadas", "eliminar")]
        public async Task<IActionResult> Eliminar(int id)
        {
            var eliminado = await _ordenCompraService.EliminarAsync(id);

            if (!eliminado)
                return NotFound();

            return NoContent();
        }
    }
}
