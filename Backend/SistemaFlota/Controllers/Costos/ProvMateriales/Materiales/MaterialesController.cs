using Microsoft.AspNetCore.Mvc;
using SistemaFlota.Authorization;
using SistemaFlota.DTOs.Costos.Materiales;
using SistemaFlota.DTOs.Prov_Materiales.Proveedores;
using SistemaFlota.Services.Costos.Materiales;

namespace SistemaFlota.Controllers.Costos.ProvMateriales.Materiales
{
    [ApiController]
    [Route("api/[controller]")]
    public class MaterialesController : ControllerBase
    {
        private readonly IMaterialesService _materialesService;

        public MaterialesController(IMaterialesService materialesService)
        {
            _materialesService = materialesService; 
        }

        /// Listar Proveedores
        [HttpGet]
        [Permiso("proveedores-materiales", "ver")]
        public async Task<ActionResult<MaterialPaginadoDto>> Obtener(
            [FromQuery] string? search,
            [FromQuery] string? estado,
            [FromQuery] string? proveedor,
            [FromQuery] string? color,
            [FromQuery] string? orden,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            var resultado = await _materialesService.ObtenerAsync(
                search,
                estado,
                orden,
                proveedor,
                color,
                page,
                pageSize
            );

            return Ok(resultado);
        }

        // Obtener material por Id
        [HttpGet("{id:int}")]
        [Permiso("proveedores-materiales", "ver")]
        public async Task<ActionResult<MaterialDto>> ObtenerPorId(int id)
        {
            var material = await _materialesService.ObtenerPorIdAsync(id);

            if (material == null)
                return NotFound();

            return Ok(material);
        }

        // Filtros
        [HttpGet("filtros")]
        [Permiso("proveedores-materiales", "ver")]
        public async Task<ActionResult<FiltrosMaterialDto>> ObtenerFiltros()
        {
            var filtros = await _materialesService.ObtenerFiltrosAsync();
            return Ok(filtros);
        }

        // Crear material
        [HttpPost]
        [Permiso("proveedores-materiales", "crear")]
        public async Task<ActionResult<MaterialDto>> Crear([FromForm] CrearMaterialDto dto)
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            var material = await _materialesService.CrearAsync(dto);

            return CreatedAtAction(
                nameof(ObtenerPorId),
                new { id = material.IdMaterial },
                material);

        }

        // Actualizar un material 
        [HttpPut("{id:int}")]
        [Permiso("proveedores-materiales", "editar")]
        public async Task<IActionResult> Actualizar(int id, [FromForm] ActualizarMaterialDto dto)
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            var actualizado = await _materialesService.ActualizarAsync(id, dto);

            if (!actualizado)
                return NotFound();

            return NoContent();
        }
    }
}
