using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaFlota.Authorization;
using SistemaFlota.DTOs.ComprasNoFormalizadas.Proveedores;
using SistemaFlota.Services.ComprasNoFormalizadas.Proveedores;

namespace SistemaFlota.Controllers.ComprasNoFormalizadas.Proveedores
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ProveedorNoFormalizadoController : ControllerBase
    {
        private readonly IProveedorNoFormalizadoService _service;

        public ProveedorNoFormalizadoController(
            IProveedorNoFormalizadoService service)
        {
            _service = service;
        }

        // Listar proveedores
        [HttpGet]
        [Permiso("proveedores-no-formalizados", "ver")]
        public async Task<ActionResult<ProveedorNoFormalizadoPaginadoDto>> Obtener(
            [FromQuery] string? search,
            [FromQuery] string? estado,
            [FromQuery] string? orden,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            var resultado = await _service.ObtenerAsync(
                search,
                estado,
                orden,
                page,
                pageSize);

            return Ok(resultado);
        }

        // Proveedores para recepción de mercancía
        [HttpGet("para-recepcion")]
        [Permiso("recepcion-compras-no-formalizadas", "ver")]
        public async Task<ActionResult<ProveedorNoFormalizadoPaginadoDto>> ObtenerParaRecepcion(
            [FromQuery] string? search,
            [FromQuery] string? estado,
            [FromQuery] string? orden,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            var resultado = await _service.ObtenerAsync(
                search,
                estado,
                orden,
                page,
                pageSize);

            return Ok(resultado);
        }

        // Obtener proveedor por ID
        [HttpGet("{id:int}")]
        [Permiso("proveedores-no-formalizados", "ver")]
        public async Task<ActionResult<ProveedorNoFormalizadoDto>> ObtenerPorId(int id)
        {
            var proveedor = await _service.ObtenerPorIdAsync(id);

            if (proveedor == null)
            {
                return NotFound(new
                {
                    message = "Proveedor no formalizado no encontrado."
                });
            }

            return Ok(proveedor);
        }

        // Crear proveedor
        [HttpPost]
        [Permiso("proveedores-no-formalizados", "crear")]
        public async Task<ActionResult<ProveedorNoFormalizadoDto>> Crear(
            [FromBody] CrearProveedorNoFormalizadoDto dto)
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            try
            {
                var proveedor = await _service.CrearAsync(dto);

                return Ok(proveedor);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new
                {
                    message = ex.Message
                });
            }
        }

        // Actualizar proveedor
        [HttpPut("{id:int}")]
        [Permiso("proveedores-no-formalizados", "editar")]
        public async Task<IActionResult> Actualizar(
            int id,
            [FromBody] ActualizarProveedorNoFormalizadoDto dto)
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            try
            {
                var actualizado = await _service.ActualizarAsync(id, dto);

                if (!actualizado)
                {
                    return NotFound(new
                    {
                        message = "Proveedor no formalizado no encontrado."
                    });
                }

                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new
                {
                    message = ex.Message
                });
            }
        }

        // Eliminar / desactivar proveedor
        [HttpDelete("{id:int}")]
        [Permiso("proveedores-no-formalizados", "eliminar")]
        public async Task<IActionResult> Eliminar(int id)
        {
            var eliminado = await _service.EliminarAsync(id);

            if (!eliminado)
                return NotFound();

            return NoContent();
        }
    }
}