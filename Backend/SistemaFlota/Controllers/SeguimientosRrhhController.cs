using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaFlota.DTOs;
using SistemaFlota.Models;
using System.Security.Claims;

namespace SistemaFlota.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class SeguimientosRrhhController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        public SeguimientosRrhhController(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // ─── GET api/seguimientosrrhh ──────────────────────────────────────────
        [HttpGet]
        public async Task<ActionResult<IEnumerable<SeguimientoRrhhDto>>> GetAll(
            [FromQuery] string? area,
            [FromQuery] string? estado,
            [FromQuery] string? prioridad,
            [FromQuery] int? mes,
            [FromQuery] int? anio)
        {
            var query = _context.SeguimientosRrhh
                .Include(s => s.Fotos)
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrEmpty(area)) query = query.Where(s => s.Area.Contains(area));
            if (!string.IsNullOrEmpty(estado)) query = query.Where(s => s.Estado == estado);
            if (!string.IsNullOrEmpty(prioridad)) query = query.Where(s => s.Prioridad == prioridad);
            if (mes.HasValue) query = query.Where(s => s.Mes == mes.Value);
            if (anio.HasValue) query = query.Where(s => s.Anio == anio.Value);

            var items = await query.OrderByDescending(s => s.FechaCreacion).ToListAsync();

            // Usar Username en lugar de Nombre + Apellido
            var userIds = items.Select(s => s.CreadoPor).Distinct().ToList();
            var usuarios = await _context.Usuarios
                .Where(u => userIds.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id, u => u.Username);

            return Ok(items.Select(s => MapToDto(s, usuarios.GetValueOrDefault(s.CreadoPor))));
        }

        // ─── GET api/seguimientosrrhh/{id} ────────────────────────────────────
        [HttpGet("{id}")]
        public async Task<ActionResult<SeguimientoRrhhDto>> GetById(int id)
        {
            var item = await _context.SeguimientosRrhh
                .Include(s => s.Fotos)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (item == null) return NotFound();

            var username = await _context.Usuarios
                .Where(u => u.Id == item.CreadoPor)
                .Select(u => u.Username)
                .FirstOrDefaultAsync();

            return Ok(MapToDto(item, username));
        }

        // ─── POST api/seguimientosrrhh ────────────────────────────────────────
        [HttpPost]
        public async Task<ActionResult<SeguimientoRrhhDto>> Create(
            [FromForm] CrearSeguimientoRrhhDto dto,
            [FromForm] List<IFormFile>? fotosEvidencia,
            [FromForm] List<IFormFile>? fotosSeguimiento)
        {
            var userId = ObtenerUserId();

            var item = new SeguimientoRrhh
            {
                Area = dto.Area,
                Mes = dto.Mes,
                Anio = dto.Anio,
                Fuente = dto.Fuente,
                Areas = dto.Areas,
                Descripcion = dto.Descripcion,
                PlanAccionSugerido = dto.PlanAccionSugerido,
                FactorRiesgo = dto.FactorRiesgo,
                Prioridad = dto.Prioridad,
                Responsable = dto.Responsable,
                FechaEjecucion = dto.FechaEjecucion,
                FechaSeguimiento = dto.FechaSeguimiento,
                Estado = dto.Estado,
                Observaciones = dto.Observaciones,
                CreadoPor = userId,
                FechaCreacion = DateTime.Now
            };

            _context.SeguimientosRrhh.Add(item);
            await _context.SaveChangesAsync();

            if (fotosEvidencia?.Any() == true)
                await GuardarFotos(fotosEvidencia, item.Id, "evidencia");

            if (fotosSeguimiento?.Any() == true)
                await GuardarFotos(fotosSeguimiento, item.Id, "seguimiento");

            await _context.Entry(item).Collection(i => i.Fotos).LoadAsync();

            return CreatedAtAction(nameof(GetById), new { id = item.Id }, MapToDto(item, null));
        }

        // ─── PUT api/seguimientosrrhh/{id} ────────────────────────────────────
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(
            int id,
            [FromForm] ActualizarSeguimientoRrhhDto dto,
            [FromForm] List<IFormFile>? fotosEvidencia,
            [FromForm] List<IFormFile>? fotosSeguimiento)
        {
            var item = await _context.SeguimientosRrhh
                .Include(s => s.Fotos)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (item == null) return NotFound();

            item.Area = dto.Area;
            item.Mes = dto.Mes;
            item.Anio = dto.Anio;
            item.Fuente = dto.Fuente;
            item.Areas = dto.Areas;
            item.Descripcion = dto.Descripcion;
            item.PlanAccionSugerido = dto.PlanAccionSugerido;
            item.FactorRiesgo = dto.FactorRiesgo;
            item.Prioridad = dto.Prioridad;
            item.Responsable = dto.Responsable;
            item.FechaEjecucion = dto.FechaEjecucion;
            item.FechaSeguimiento = dto.FechaSeguimiento;
            item.Estado = dto.Estado;
            item.Observaciones = dto.Observaciones;
            item.ModificadoPor = ObtenerUserId();
            item.FechaModificacion = DateTime.Now;

            if (fotosEvidencia?.Any() == true)
                await GuardarFotos(fotosEvidencia, item.Id, "evidencia");

            if (fotosSeguimiento?.Any() == true)
                await GuardarFotos(fotosSeguimiento, item.Id, "seguimiento");

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // ─── DELETE api/seguimientosrrhh/{id} ─────────────────────────────────
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,Jefe,RecursosHumanos")]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _context.SeguimientosRrhh
                .Include(s => s.Fotos)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (item == null) return NotFound();

            foreach (var foto in item.Fotos)
                EliminarArchivo(foto.NombreArchivo);

            _context.SeguimientosRrhh.Remove(item);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // ─── DELETE api/seguimientosrrhh/foto/{fotoId} ────────────────────────
        [HttpDelete("foto/{fotoId}")]
        public async Task<IActionResult> DeleteFoto(int fotoId)
        {
            var foto = await _context.SeguimientosRrhhFotos.FindAsync(fotoId);
            if (foto == null) return NotFound();

            EliminarArchivo(foto.NombreArchivo);
            _context.SeguimientosRrhhFotos.Remove(foto);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // ─── Helpers ──────────────────────────────────────────────────────────
        private async Task GuardarFotos(List<IFormFile> fotos, int seguimientoId, string tipo)
        {
            var carpeta = Path.Combine(_env.WebRootPath, "seguimientos-rrhh");
            if (!Directory.Exists(carpeta)) Directory.CreateDirectory(carpeta);

            foreach (var foto in fotos.Where(f => f.Length > 0))
            {
                var ext = Path.GetExtension(foto.FileName).ToLowerInvariant();
                var nombre = $"{tipo}_{seguimientoId}_{DateTime.Now:yyyyMMddHHmmssfff}_{Guid.NewGuid():N}{ext}";
                var ruta = Path.Combine(carpeta, nombre);

                using var stream = new FileStream(ruta, FileMode.Create);
                await foto.CopyToAsync(stream);

                _context.SeguimientosRrhhFotos.Add(new SeguimientoRrhhFoto
                {
                    SeguimientoId = seguimientoId,
                    NombreArchivo = nombre,
                    TipoFoto = tipo,
                    FechaSubida = DateTime.Now
                });
            }
            await _context.SaveChangesAsync();
        }

        private void EliminarArchivo(string nombreArchivo)
        {
            var ruta = Path.Combine(_env.WebRootPath, "seguimientos-rrhh", nombreArchivo);
            if (System.IO.File.Exists(ruta)) System.IO.File.Delete(ruta);
        }

        private int ObtenerUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                     ?? User.FindFirst("sub")?.Value ?? "0";
            return int.TryParse(claim, out var id) ? id : 0;
        }

        private static SeguimientoRrhhDto MapToDto(SeguimientoRrhh s, string? nombreCreador) => new()
        {
            Id = s.Id,
            Area = s.Area,
            Mes = s.Mes,
            Anio = s.Anio,
            Fuente = s.Fuente,
            Areas = s.Areas,
            Descripcion = s.Descripcion,
            PlanAccionSugerido = s.PlanAccionSugerido,
            FactorRiesgo = s.FactorRiesgo,
            Prioridad = s.Prioridad,
            Responsable = s.Responsable,
            FechaEjecucion = s.FechaEjecucion,
            FechaSeguimiento = s.FechaSeguimiento,
            Estado = s.Estado,
            Observaciones = s.Observaciones,
            CreadoPor = s.CreadoPor,
            NombreCreadoPor = nombreCreador,
            FechaCreacion = s.FechaCreacion,
            ModificadoPor = s.ModificadoPor,
            FechaModificacion = s.FechaModificacion,
            Fotos = s.Fotos.Select(f => new SeguimientoRrhhFotoDto
            {
                Id = f.Id,
                NombreArchivo = f.NombreArchivo,
                TipoFoto = f.TipoFoto,
                FechaSubida = f.FechaSubida
            }).ToList()
        };
    }
}