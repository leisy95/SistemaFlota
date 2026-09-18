using Microsoft.EntityFrameworkCore;
using SistemaFlota.Models.Calidad;

namespace SistemaFlota.Repositories.Calidad
{
    public class SalidaNoConformeRepository : ISalidaNoConformeRepository
    {
        private readonly AppDbContext _context;

        public SalidaNoConformeRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<SalidaNoConforme>> ObtenerTodosAsync(
            DateTime? desde, DateTime? hasta, string? referencia,
            string? material, string? ordenProduccion, string? tipoDefecto, string? proceso)
        {
            var query = _context.Set<SalidaNoConforme>().AsQueryable();

            if (desde.HasValue) query = query.Where(s => s.FechaReporte >= desde.Value);
            if (hasta.HasValue) query = query.Where(s => s.FechaReporte <= hasta.Value.AddDays(1));
            if (!string.IsNullOrWhiteSpace(referencia)) query = query.Where(s => s.Referencia != null && s.Referencia.Contains(referencia));
            if (!string.IsNullOrWhiteSpace(material)) query = query.Where(s => s.Material == material);
            if (!string.IsNullOrWhiteSpace(ordenProduccion)) query = query.Where(s => s.OrdenProduccion.Contains(ordenProduccion));
            if (!string.IsNullOrWhiteSpace(tipoDefecto)) query = query.Where(s => s.TipoDefecto == tipoDefecto);
            if (!string.IsNullOrWhiteSpace(proceso)) query = query.Where(s => s.Proceso == proceso);

            return await query.OrderByDescending(s => s.FechaReporte).ToListAsync();
        }

        public async Task<SalidaNoConforme?> ObtenerPorIdAsync(int id)
            => await _context.Set<SalidaNoConforme>().FindAsync(id);

        public async Task AgregarAsync(SalidaNoConforme entidad)
            => await _context.Set<SalidaNoConforme>().AddAsync(entidad);

        public async Task GuardarCambiosAsync()
            => await _context.SaveChangesAsync();

        public void Eliminar(SalidaNoConforme entidad)
            => _context.Set<SalidaNoConforme>().Remove(entidad);

        public async Task AgregarEvidenciasAsync(List<SalidaNoConformeEvidencia> evidencias)
        {
            await _context.Set<SalidaNoConformeEvidencia>().AddRangeAsync(evidencias);
            await _context.SaveChangesAsync();
        }

        public async Task<List<SalidaNoConformeEvidencia>> ObtenerEvidenciasAsync(int salidaNoConformeId)
        {
            return await _context.Set<SalidaNoConformeEvidencia>()
                .Where(e => e.SalidaNoConformeId == salidaNoConformeId)
                .OrderBy(e => e.FechaSubida)
                .ToListAsync();
        }
    }
}