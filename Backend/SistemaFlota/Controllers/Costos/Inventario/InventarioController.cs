using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaFlota.Services.Costos.Inventario;

namespace SistemaFlota.Controllers.Costos.Inventario
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class InventarioController : ControllerBase
    {
        private readonly IInventarioService _inventarioService;

        public InventarioController(IInventarioService inventarioService)
        {
            _inventarioService = inventarioService;
        }

        [HttpGet]
        public async Task<IActionResult> Obtener(
            string? search,
            int? proveedorId,
            string? categoria,
            string? color,
            int page = 1,
            int pageSize = 20)
        {
            var inventario = await _inventarioService.ObtenerAsync(
                search,
                proveedorId,
                categoria,
                color,
                page,
                pageSize);

            return Ok(inventario);
        }

        [HttpGet("proveedores")]
        public async Task<IActionResult> ObtenerProveedores()
        {
            return Ok(await _inventarioService.ObtenerProveedoresInventarioAsync());
        }

        [HttpGet("categorias")]
        public async Task<IActionResult> ObtenerCategorias()
        {
            return Ok(await _inventarioService.ObtenerCategoriasInventarioAsync());
        }

        [HttpGet("excel")]
        public async Task<IActionResult> ExportarExcel(
            string? search,
            int? proveedorId,
            string? categoria,
            string? color)
        {
            var archivo = await _inventarioService.ExportarExcelAsync(
                search,
                proveedorId,
                categoria,
                color);

            return File(
                archivo,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"Inventario_{DateTime.Now:yyyyMMddHHmmss}.xlsx");
        }
    }
}