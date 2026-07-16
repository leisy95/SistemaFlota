using Microsoft.AspNetCore.Mvc;
using SistemaFlota.DTOs.Prov_Materiales.Proveedores;
using SistemaFlota.Services.Costos.Proveedores;

namespace SistemaFlota.Controllers.Prov_Materiales
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProveedoresController : ControllerBase
    {
        private readonly IProveedorService _proveedorService;

        public ProveedoresController(IProveedorService proveedorService)
        {
            _proveedorService = proveedorService;
        }

        /// Listar Proveedores
        [HttpGet]
        public async Task<ActionResult<ProveedorPaginadoDto>> Obtener(
            [FromQuery] string? search,
            [FromQuery] string? estado,
            [FromQuery] string? orden,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            var resultado = await _proveedorService.ObtenerAsync(
                search,
                estado,
                orden,
                page,
                pageSize
            );

            return Ok(resultado);
        }

        /// Obtiene un proveedor por su Id. para editar
        [HttpGet("{id:int}")]
        public async Task<ActionResult<ProveedorDto>> ObtenerPorId(int id)
        {
            var proveedor = await _proveedorService.ObtenerPorIdAsync(id);

            if (proveedor == null)
                return NotFound();

            return Ok(proveedor);
        }

        /// Crea un nuevo proveedor.
        [HttpPost]
        public async Task<ActionResult<ProveedorDto>> Crear([FromBody] CrearProveedorDto dto)
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            var proveedor = await _proveedorService.CrearAsync(dto);

            return CreatedAtAction(
                nameof(ObtenerPorId),
                new { id = proveedor.IdProveedor },
                proveedor);
        }

        /// Actualiza un proveedor.
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Actualizar(int id, [FromBody] ActualizarProveedorDto dto)
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            var actualizado = await _proveedorService.ActualizarAsync(id, dto);

            if (!actualizado)
                return NotFound();

            return NoContent();
        }

        /// Elimina (desactiva) un proveedor.
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Eliminar(int id)
        {
            var eliminado = await _proveedorService.EliminarAsync(id);

            if (!eliminado)
                return NotFound();

            return NoContent();
        }
    }
}
