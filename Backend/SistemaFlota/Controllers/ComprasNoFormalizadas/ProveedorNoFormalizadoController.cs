using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaFlota.DTOs.ComprasNoFormalizadas.Proveedores;
using SistemaFlota.Services.ComprasNoFormalizadas.Proveedores;

namespace SistemaFlota.Controllers.ComprasNoFormalizadas
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ProveedorNoFormalizadoController : ControllerBase
    {
        private readonly IProveedorNoFormalizadoService _service;

        public ProveedorNoFormalizadoController(IProveedorNoFormalizadoService service)
        {
            _service = service;
        }

        // GET: api/ProveedorNoFormalizado
        [HttpGet]
        public async Task<IActionResult> Obtener(
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
                pageSize
            );

            return Ok(resultado);
        }

        // GET: api/ProveedorNoFormalizado/5
        [HttpGet("{id}")]
        public async Task<IActionResult> ObtenerPorId(int id)
        {
            var proveedor = await _service.ObtenerPorIdAsync(id);

            if (proveedor == null)
                return NotFound(new { message = "Proveedor no formalizado no encontrado." });

            return Ok(proveedor);
        }

        // POST: api/ProveedorNoFormalizado
        [HttpPost]
        public async Task<IActionResult> Crear(
            [FromBody] CrearProveedorNoFormalizadoDto dto)
        {
            try
            {
                var proveedor = await _service.CrearAsync(dto);

                return Ok(proveedor);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // PUT: api/ProveedorNoFormalizado/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Actualizar(
            int id,
            [FromBody] ActualizarProveedorNoFormalizadoDto dto)
        {
            try
            {
                var actualizado = await _service.ActualizarAsync(id, dto);

                if (!actualizado)
                    return NotFound(new { message = "Proveedor no formalizado no encontrado." });

                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}