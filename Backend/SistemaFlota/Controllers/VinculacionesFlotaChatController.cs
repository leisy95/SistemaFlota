using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace SistemaFlota.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class VinculacionesFlotaChatController : ControllerBase
    {
        private readonly AppDbContext _context;

        public VinculacionesFlotaChatController(AppDbContext context)
        {
            _context = context;
        }

        // ── GET: listar vinculaciones existentes por tipo ──────────────────
        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] string? tipoEntidad = null)
        {
            var query = _context.VinculacionesFlotaChat.AsQueryable();
            if (!string.IsNullOrEmpty(tipoEntidad))
                query = query.Where(v => v.TipoEntidad == tipoEntidad);

            var lista = await query.ToListAsync();
            return Ok(lista);
        }

        // ── GET: conductores sin vincular (comparando con FlotaChat) ───────
        [HttpGet("conductores-pendientes")]
        public async Task<IActionResult> GetConductoresPendientes()
        {
            var conductoresVinculados = await _context.VinculacionesFlotaChat
                .Where(v => v.TipoEntidad == "Conductor")
                .Select(v => v.EntidadId)
                .ToListAsync();

            var conductores = await _context.Conductores
                .Where(c => !conductoresVinculados.Contains(c.Id))
                .Select(c => new { c.Id, c.Nombre, c.Telefono })
                .ToListAsync();

            return Ok(conductores);
        }

        // ── POST: crear vinculación ─────────────────────────────────────────
        [HttpPost]
        public async Task<IActionResult> Crear([FromBody] CrearVinculacionDto dto)
        {
            var existe = await _context.VinculacionesFlotaChat
                .AnyAsync(v => v.FlotaChatUsuarioId == dto.FlotaChatUsuarioId);
            if (existe)
                return BadRequest(new { mensaje = "Este usuario de FlotaChat ya está vinculado" });

            var vinculacion = new VinculacionFlotaChat
            {
                FlotaChatUsuarioId = dto.FlotaChatUsuarioId,
                TipoEntidad = dto.TipoEntidad,
                EntidadId = dto.EntidadId,
                Telefono = dto.Telefono,
                FechaVinculacion = DateTime.Now
            };

            _context.VinculacionesFlotaChat.Add(vinculacion);
            await _context.SaveChangesAsync();

            return Ok(vinculacion);
        }

        // ── DELETE: quitar vinculación ──────────────────────────────────────
        [HttpDelete("{id}")]
        public async Task<IActionResult> Eliminar(int id)
        {
            var vinculacion = await _context.VinculacionesFlotaChat.FindAsync(id);
            if (vinculacion == null) return NotFound();

            _context.VinculacionesFlotaChat.Remove(vinculacion);
            await _context.SaveChangesAsync();
            return Ok();
        }

        // ── Helper interno: buscar Conductor por FlotaChatUsuarioId ─────────
        // (lo usará el webhook y el BackgroundService más adelante)
        public static async Task<int?> ObtenerConductorId(AppDbContext context, int flotaChatUsuarioId)
        {
            var vinculacion = await context.VinculacionesFlotaChat
                .FirstOrDefaultAsync(v => v.FlotaChatUsuarioId == flotaChatUsuarioId && v.TipoEntidad == "Conductor");
            return vinculacion?.EntidadId;
        }
    }

    public record CrearVinculacionDto(
        int FlotaChatUsuarioId,
        string TipoEntidad,
        int EntidadId,
        string? Telefono
    );
}