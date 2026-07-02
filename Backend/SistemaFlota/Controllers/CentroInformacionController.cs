using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace SistemaFlota
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CentroInformacionController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly AuditoriaService _auditoria;

        public CentroInformacionController(AppDbContext context, AuditoriaService auditoria)
        {
            _context = context;
            _auditoria = auditoria;
        }

        private string GetUsuario() => User.FindFirst(ClaimTypes.Name)?.Value ?? "Desconocido";
        private string GetRol() => User.FindFirst(ClaimTypes.Role)?.Value ?? "Desconocido";

        // ── INVENTARIO RUTAS ──────────────────────────────────────────────
        [HttpGet("rutas")]
        public async Task<IActionResult> GetRutas()
        {
            var rutas = await _context.InventarioRutas.OrderByDescending(r => r.FechaCreacion).ToListAsync();
            return Ok(rutas);
        }

        [HttpPost("rutas")]
        public async Task<IActionResult> CrearRuta([FromForm] string Nombre, [FromForm] string? Descripcion, IFormFile? archivo)
        {
            string? nombreArchivo = null;
            if (archivo != null)
            {
                var carpeta = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/rutas");
                if (!Directory.Exists(carpeta)) Directory.CreateDirectory(carpeta);
                nombreArchivo = Guid.NewGuid().ToString() + Path.GetExtension(archivo.FileName);
                using var stream = new FileStream(Path.Combine(carpeta, nombreArchivo), FileMode.Create);
                await archivo.CopyToAsync(stream);
            }
            var ruta = new InventarioRuta { Nombre = Nombre, Descripcion = Descripcion, Archivo = nombreArchivo, CreadoPor = GetUsuario() };
            _context.InventarioRutas.Add(ruta);
            await _context.SaveChangesAsync();
            await _auditoria.RegistrarAsync(GetUsuario(), GetRol(), "Crear", "CentroInformacion", $"Ruta creada: {Nombre}", ruta.Id);
            return Ok(ruta);
        }

        [HttpDelete("rutas/{id}")]
        public async Task<IActionResult> EliminarRuta(int id)
        {
            var ruta = await _context.InventarioRutas.FindAsync(id);
            if (ruta == null) return NotFound();
            _context.InventarioRutas.Remove(ruta);
            await _context.SaveChangesAsync();
            return Ok();
        }

        // ── NUMEROS EMERGENCIA ────────────────────────────────────────────
        [HttpGet("emergencia")]
        public async Task<IActionResult> GetEmergencia()
        {
            var numeros = await _context.NumerosEmergencia.Where(n => n.Activo).OrderBy(n => n.Ciudad).ToListAsync();
            return Ok(numeros);
        }

        [HttpPost("emergencia")]
        public async Task<IActionResult> CrearEmergencia([FromBody] NumeroEmergencia dto)
        {
            dto.Id = 0;
            _context.NumerosEmergencia.Add(dto);
            await _context.SaveChangesAsync();
            await _auditoria.RegistrarAsync(GetUsuario(), GetRol(), "Crear", "CentroInformacion", $"Numero emergencia: {dto.Ciudad} - {dto.Tipo}", dto.Id);
            return Ok(dto);
        }

        [HttpPut("emergencia/{id}")]
        public async Task<IActionResult> ActualizarEmergencia(int id, [FromBody] NumeroEmergencia dto)
        {
            var num = await _context.NumerosEmergencia.FindAsync(id);
            if (num == null) return NotFound();
            num.Ciudad = dto.Ciudad;
            num.Tipo = dto.Tipo;
            num.Numero = dto.Numero;
            num.Observaciones = dto.Observaciones;
            await _context.SaveChangesAsync();
            return Ok(num);
        }

        [HttpDelete("emergencia/{id}")]
        public async Task<IActionResult> EliminarEmergencia(int id)
        {
            var num = await _context.NumerosEmergencia.FindAsync(id);
            if (num == null) return NotFound();
            num.Activo = false;
            await _context.SaveChangesAsync();
            return Ok();
        }
    }
}
