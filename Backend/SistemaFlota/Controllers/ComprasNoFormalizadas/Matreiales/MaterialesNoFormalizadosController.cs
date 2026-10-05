using Microsoft.AspNetCore.Mvc;
using SistemaFlota.Authorization;
using SistemaFlota.DTOs.ComprasNoFormalizadas.Material;
using SistemaFlota.Services.ComprasNoFormalizadas.Materiales;

namespace SistemaFlota.Controllers.ComprasNoFormalizadas.Matreiales
{
    [ApiController]
    [Route("api/[controller]")]
    public class MaterialesNoFormalizadosController : ControllerBase
    {
        private readonly IMaterialNoFormalizadoService _materialesService;

        public MaterialesNoFormalizadosController(IMaterialNoFormalizadoService materialesService)
        {
            _materialesService = materialesService;
        }

        /// Listar materiales no formalizados
        [HttpGet]
        [Permiso("proveedores-materiales", "ver")]
        public async Task<ActionResult<MaterialNoFormalizadoPaginadoDto>> Obtener(
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
        public async Task<ActionResult<MaterialNoFormalizadoDto>> ObtenerPorId(int id)
        {
            var material = await _materialesService.ObtenerPorIdAsync(id);

            if (material == null)
                return NotFound();

            return Ok(material);
        }

        // Obtener Colores
        [HttpGet("colores")]
        [Permiso("proveedores-materiales", "ver")]
        public async Task<IActionResult> ObtenerColores()
        {
            var colores = await _materialesService.ObtenerColoresAsync();

            return Ok(colores);
        }

        // Obtener Categorías
        [HttpGet("categorias")]
        [Permiso("proveedores-materiales", "ver")]
        public async Task<IActionResult> ObtenerCategorias()
        {
            var categorias = await _materialesService.ObtenerCategoriasAsync();

            return Ok(categorias);
        }

        // Filtros
        [HttpGet("filtros")]
        [Permiso("proveedores-materiales", "ver")]
        public async Task<ActionResult<FiltrosMaterialNoFormalizadoDto>> ObtenerFiltros()
        {
            var filtros = await _materialesService.ObtenerFiltrosAsync();

            return Ok(filtros);
        }

        // Crear material
        [HttpPost]
        [Permiso("proveedores-materiales", "crear")]
        public async Task<ActionResult<MaterialNoFormalizadoDto>> Crear(
            [FromForm] CrearMaterialNoFormalizadoDto dto)
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            var material = await _materialesService.CrearAsync(dto);

            return CreatedAtAction(
                nameof(ObtenerPorId),
                new { id = material.IdMaterialNoFormalizado },
                material);
        }

        // Actualizar material
        [HttpPut("{id:int}")]
        [Permiso("proveedores-materiales", "editar")]
        public async Task<IActionResult> Actualizar(
            int id,
            [FromForm] ActualizarMaterialNoFormalizadoDto dto)
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
