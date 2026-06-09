using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaFlota.Models;
using System.Security.Claims;

namespace SistemaFlota.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CyrelesController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        public CyrelesController(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        private int GetUserId()
        {
            var v = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0";
            return int.TryParse(v, out var id) ? id : 0;
        }

        // ── GET api/cyreles/cajones ───────────────────────────────────────────
        [HttpGet("cajones")]
        public async Task<IActionResult> GetCajones()
        {
            var cajones = await _context.Cajones
                .Where(c => c.Activo)
                .OrderBy(c => c.Numero)
                .Select(c => new {
                    c.Id,
                    c.Numero,
                    c.Descripcion,
                    TotalRegistros = c.Registros.Count
                })
                .ToListAsync();
            return Ok(cajones);
        }

        // ── POST api/cyreles/cajones ──────────────────────────────────────────
        [HttpPost("cajones")]
        public async Task<IActionResult> CrearCajon([FromBody] CajonDto dto)
        {
            var existe = await _context.Cajones.AnyAsync(c => c.Numero == dto.Numero);
            if (existe) return BadRequest($"El cajón {dto.Numero} ya existe");

            var cajon = new Cajon { Numero = dto.Numero, Descripcion = dto.Descripcion };
            _context.Cajones.Add(cajon);
            await _context.SaveChangesAsync();
            return Ok(cajon);
        }

        // ── PUT api/cyreles/cajones/{id} ──────────────────────────────────────
        [HttpPut("cajones/{id}")]
        public async Task<IActionResult> EditarCajon(int id, [FromBody] CajonDto dto)
        {
            var cajon = await _context.Cajones.FindAsync(id);
            if (cajon == null) return NotFound();
            cajon.Numero = dto.Numero;
            cajon.Descripcion = dto.Descripcion;
            await _context.SaveChangesAsync();
            return Ok(cajon);
        }

        // ── DELETE api/cyreles/cajones/{id} ───────────────────────────────────
        [HttpDelete("cajones/{id}")]
        public async Task<IActionResult> EliminarCajon(int id)
        {
            var cajon = await _context.Cajones
                .Include(c => c.Registros)
                .FirstOrDefaultAsync(c => c.Id == id);
            if (cajon == null) return NotFound();
            foreach (var r in cajon.Registros) EliminarFoto(r.Foto);
            _context.Cajones.Remove(cajon);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // ── GET api/cyreles/registros ─────────────────────────────────────────
        [HttpGet("registros")]
        public async Task<IActionResult> GetRegistros(
            [FromQuery] int? cajonId,
            [FromQuery] string? nombre,
            [FromQuery] int? cajonNumero)
        {
            var query = _context.CyreleRegistros
                .Include(r => r.Cajon)
                .AsNoTracking()
                .AsQueryable();

            if (cajonId.HasValue) query = query.Where(r => r.CajonId == cajonId.Value);
            if (cajonNumero.HasValue) query = query.Where(r => r.Cajon!.Numero == cajonNumero.Value);
            if (!string.IsNullOrEmpty(nombre)) query = query.Where(r => r.Nombre.Contains(nombre));

            var result = await query
                .OrderBy(r => r.Cajon!.Numero)
                .ThenBy(r => r.Nombre)
                .Select(r => new {
                    r.Id,
                    r.CajonId,
                    r.Nombre,
                    r.Foto,
                    r.FechaCreacion,
                    r.CreadoPor,
                    CajonNumero = r.Cajon!.Numero,
                    CajonDescripcion = r.Cajon.Descripcion
                })
                .ToListAsync();

            return Ok(result);
        }

        // ── GET api/cyreles/registros/{id} ────────────────────────────────────
        [HttpGet("registros/{id}")]
        public async Task<IActionResult> GetRegistro(int id)
        {
            var r = await _context.CyreleRegistros
                .Include(r => r.Cajon)
                .FirstOrDefaultAsync(r => r.Id == id);
            if (r == null) return NotFound();
            return Ok(new
            {
                r.Id,
                r.CajonId,
                r.Nombre,
                r.Foto,
                r.FechaCreacion,
                r.CreadoPor,
                CajonNumero = r.Cajon!.Numero,
                CajonDescripcion = r.Cajon.Descripcion
            });
        }

        // ── POST api/cyreles/registros ────────────────────────────────────────
        [HttpPost("registros")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> CrearRegistro(
            [FromForm] RegistroDto dto,
            [FromForm] IFormFile? foto)
        {
            var registro = new CyreleRegistro
            {
                CajonId = dto.CajonId,
                Nombre = dto.Nombre,
                CreadoPor = GetUserId(),
                FechaCreacion = DateTime.Now
            };

            if (foto != null && foto.Length > 0)
                registro.Foto = await GuardarFoto(foto);

            _context.CyreleRegistros.Add(registro);
            await _context.SaveChangesAsync();
            return Ok(registro);
        }

        // ── PUT api/cyreles/registros/{id} ────────────────────────────────────
        [HttpPut("registros/{id}")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> EditarRegistro(
            int id,
            [FromForm] RegistroDto dto,
            [FromForm] IFormFile? foto)
        {
            var registro = await _context.CyreleRegistros.FindAsync(id);
            if (registro == null) return NotFound();

            registro.CajonId = dto.CajonId;
            registro.Nombre = dto.Nombre;
            registro.ModificadoPor = GetUserId();
            registro.FechaModificacion = DateTime.Now;

            if (foto != null && foto.Length > 0)
            {
                EliminarFoto(registro.Foto);
                registro.Foto = await GuardarFoto(foto);
            }

            await _context.SaveChangesAsync();
            return Ok(registro);
        }

        // ── DELETE api/cyreles/registros/{id} ─────────────────────────────────
        [HttpDelete("registros/{id}")]
        public async Task<IActionResult> EliminarRegistro(int id)
        {
            var registro = await _context.CyreleRegistros.FindAsync(id);
            if (registro == null) return NotFound();
            EliminarFoto(registro.Foto);
            _context.CyreleRegistros.Remove(registro);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // ── Helpers ───────────────────────────────────────────────────────────
        private async Task<string> GuardarFoto(IFormFile foto)
        {
            var carpeta = Path.Combine(_env.WebRootPath, "cyreles");
            if (!Directory.Exists(carpeta)) Directory.CreateDirectory(carpeta);
            var ext = Path.GetExtension(foto.FileName).ToLowerInvariant();
            var nombre = $"cyrele_{DateTime.Now:yyyyMMddHHmmssfff}_{Guid.NewGuid():N}{ext}";
            using var stream = new FileStream(Path.Combine(carpeta, nombre), FileMode.Create);
            await foto.CopyToAsync(stream);
            return nombre;
        }

        private void EliminarFoto(string? nombreArchivo)
        {
            if (string.IsNullOrEmpty(nombreArchivo)) return;
            var ruta = Path.Combine(_env.WebRootPath, "cyreles", nombreArchivo);
            if (System.IO.File.Exists(ruta)) System.IO.File.Delete(ruta);
        }
    }

    // ── DTOs ─────────────────────────────────────────────────────────────────
    public class CajonDto
    {
        public int Numero { get; set; }
        public string? Descripcion { get; set; }
    }

    public class RegistroDto
    {
        public int CajonId { get; set; }
        public string Nombre { get; set; } = string.Empty;
    }
}