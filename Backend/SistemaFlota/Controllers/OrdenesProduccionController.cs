using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace SistemaFlota.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class OrdenesProduccionController : ControllerBase
    {
        private readonly IProveedorOrdenesProduccion _proveedor;

        public OrdenesProduccionController(IProveedorOrdenesProduccion proveedor)
        {
            _proveedor = proveedor;
        }

        // GET api/ordenesproduccion/buscar/{numeroOP}
        [HttpGet("buscar/{numeroOP}")]
        public async Task<IActionResult> Buscar(string numeroOP)
        {
            var orden = await _proveedor.BuscarPorNumero(numeroOP);
            if (orden == null) return NotFound();
            return Ok(orden);
        }

        // POST api/ordenesproduccion/importar
        [HttpPost("importar")]
        public async Task<IActionResult> Importar(IFormFile archivo)
        {
            if (archivo == null || archivo.Length == 0)
                return BadRequest(new { mensaje = "Debe adjuntar un archivo" });

            try
            {
                using var stream = archivo.OpenReadStream();
                var resultado = await _proveedor.ImportarDesdeArchivo(stream, archivo.FileName);
                return Ok(new { mensaje = $"Se importaron/actualizaron {resultado.Count} órdenes de producción", total = resultado.Count });
            }
            catch (Exception ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }
    }
}