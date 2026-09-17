using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaFlota.Authorization;
using SistemaFlota.Services.Costos.Inventario;

namespace SistemaFlota.Controllers.Costos.Inventario
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class InventarioController : ControllerBase
    {
        private readonly IInventarioService _inventarioService;
        private readonly AppDbContext _context;

        public InventarioController(
            IInventarioService inventarioService,
            AppDbContext context)
        {
            _inventarioService = inventarioService;
            _context = context;
        }

        private async Task<bool> PuedeVerDatosNumericosAsync()
        {
            var username = User.Identity?.Name;

            if (string.IsNullOrWhiteSpace(username))
                return false;

            return await _context.UsuarioPermisos
                .AnyAsync(p =>
                    p.Usuario != null &&
                    p.Usuario.Username == username &&
                    p.Modulo == "inventario" &&
                    p.PuedeVerDatosNumericos);
        }

        [HttpGet]
        [Permiso("inventario", "ver")]
        public async Task<IActionResult> Obtener(
            string? search,
            int? proveedorId,
            string? categoria,
            string? color,
            int page = 1,
            int pageSize = 20)
        {
            var puedeVerDatosNumericos =
                await PuedeVerDatosNumericosAsync();

            var inventario = await _inventarioService.ObtenerAsync(
                search,
                proveedorId,
                categoria,
                color,
                page,
                pageSize,
                puedeVerDatosNumericos);

            return Ok(inventario);
        }

        [HttpGet("proveedores")]
        [Permiso("inventario", "ver")]
        public async Task<IActionResult> ObtenerProveedores()
        {
            return Ok(
                await _inventarioService
                    .ObtenerProveedoresInventarioAsync());
        }

        [HttpGet("categorias")]
        [Permiso("inventario", "ver")]
        public async Task<IActionResult> ObtenerCategorias()
        {
            return Ok(
                await _inventarioService
                    .ObtenerCategoriasInventarioAsync());
        }

        [HttpGet("excel")]
        [Permiso("inventario", "ver")]
        public async Task<IActionResult> ExportarExcel(
            string? search,
            int? proveedorId,
            string? categoria,
            string? color)
        {
            var puedeVerDatosNumericos =
                await PuedeVerDatosNumericosAsync();

            var archivo = await _inventarioService.ExportarExcelAsync(
                search,
                proveedorId,
                categoria,
                color,
                puedeVerDatosNumericos);

            return File(
                archivo,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"Inventario_{DateTime.Now:yyyyMMddHHmmss}.xlsx");
        }
    }
}