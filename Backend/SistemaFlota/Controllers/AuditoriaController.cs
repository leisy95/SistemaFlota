using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace SistemaFlota
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AuditoriaController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AuditoriaController(AppDbContext context)
        {
            _context = context;
        }

        // =====================================
        // GET TODOS CON FILTROS
        // =====================================

        [HttpGet]
        public async Task<IActionResult> Get(
            [FromQuery] string? usuario,
            [FromQuery] string? modulo,
            [FromQuery] string? accion,
            [FromQuery] string? resultado,
            [FromQuery] DateTime? fechaDesde,
            [FromQuery] DateTime? fechaHasta,
            [FromQuery] int pagina = 1,
            [FromQuery] int porPagina = 50
        )
        {
            var query = _context.Auditorias.AsQueryable();

            if (!string.IsNullOrEmpty(usuario))
                query = query.Where(a => a.Usuario.Contains(usuario));

            if (!string.IsNullOrEmpty(modulo))
                query = query.Where(a => a.Modulo == modulo);

            if (!string.IsNullOrEmpty(accion))
                query = query.Where(a => a.Accion == accion);

            if (!string.IsNullOrEmpty(resultado))
                query = query.Where(a => a.Resultado == resultado);

            if (fechaDesde.HasValue)
                query = query.Where(a => a.Fecha >= fechaDesde.Value);

            if (fechaHasta.HasValue)
                query = query.Where(a => a.Fecha <= fechaHasta.Value.AddDays(1));

            var total = await query.CountAsync();

            var lista = await query
                .OrderByDescending(a => a.Fecha)
                .Skip((pagina - 1) * porPagina)
                .Take(porPagina)
                .ToListAsync();

            return Ok(new
            {
                total,
                pagina,
                porPagina,
                totalPaginas = (int)Math.Ceiling((double)total / porPagina),
                datos = lista
            });
        }

        // =====================================
        // GET ESTADÍSTICAS
        // =====================================

        [HttpGet("estadisticas")]
        public async Task<IActionResult> GetEstadisticas()
        {
            var hoy = DateTime.Today;

            var porModulo = await _context.Auditorias
                .GroupBy(a => a.Modulo)
                .Select(g => new { modulo = g.Key, total = g.Count() })
                .ToListAsync();

            var porAccion = await _context.Auditorias
                .GroupBy(a => a.Accion)
                .Select(g => new { accion = g.Key, total = g.Count() })
                .ToListAsync();

            var porUsuario = await _context.Auditorias
                .GroupBy(a => a.Usuario)
                .Select(g => new { usuario = g.Key, total = g.Count() })
                .OrderByDescending(g => g.total)
                .Take(10)
                .ToListAsync();

            var actividadHoy = await _context.Auditorias
                .Where(a => a.Fecha >= hoy)
                .CountAsync();

            var fallidosHoy = await _context.Auditorias
                .Where(a => a.Fecha >= hoy && a.Resultado == "Fallido")
                .CountAsync();

            return Ok(new
            {
                porModulo,
                porAccion,
                porUsuario,
                actividadHoy,
                fallidosHoy,
                totalRegistros = await _context.Auditorias.CountAsync()
            });
        }

        // =====================================
        // DELETE — LIMPIAR REGISTROS ANTIGUOS
        // =====================================

        [HttpDelete("limpiar")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Limpiar([FromQuery] int dias = 90)
        {
            var fecha = DateTime.Now.AddDays(-dias);
            var registros = await _context.Auditorias
                .Where(a => a.Fecha < fecha)
                .ToListAsync();

            _context.Auditorias.RemoveRange(registros);
            await _context.SaveChangesAsync();

            return Ok(new { eliminados = registros.Count });
        }
    }
}