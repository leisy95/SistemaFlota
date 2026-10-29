using Microsoft.EntityFrameworkCore;
using SistemaFlota.DTOs.Auditorias;

namespace SistemaFlota.Services.Auditorias
{
    public class AuditoriaService : IAuditoriaService
    {
        private readonly AppDbContext _context;
        private readonly global::SistemaFlota.AuditoriaService _auditoriaActual;

        public AuditoriaService(
            AppDbContext context,
            global::SistemaFlota.AuditoriaService auditoriaActual)
        {
            _context = context;
            _auditoriaActual = auditoriaActual;
        }

        // Registrar movimiento usando la lógica existente
        public Task RegistrarAsync(
            string usuario,
            string rol,
            string accion,
            string modulo,
            string? detalle = null,
            int? registroId = null,
            string resultado = "Exitoso")
        {
            return _auditoriaActual.RegistrarAsync(
                usuario,
                rol,
                accion,
                modulo,
                detalle,
                registroId,
                resultado);
        }

        // Consultar historial con filtros
        public async Task<List<AuditoriaDto>> ObtenerTodosAsync(
            AuditoriaFiltroDto filtro)
        {
            var query = AplicarFiltros(filtro);

            return await query
                .OrderByDescending(a => a.Fecha)
                .Select(a => new AuditoriaDto
                {
                    Id = a.Id,
                    Usuario = a.Usuario,
                    Rol = a.Rol,
                    Accion = a.Accion,
                    Modulo = a.Modulo,
                    Detalle = a.Detalle,
                    RegistroId = a.RegistroId,
                    Fecha = a.Fecha,
                    IpAddress = a.IpAddress,
                    Resultado = a.Resultado
                })
                .ToListAsync();
        }

        // Consultar historial paginado
        public async Task<AuditoriaPaginadaDto> ObtenerPaginadoAsync(
            AuditoriaFiltroDto filtro,
            int pagina = 1,
            int porPagina = 25)
        {
            pagina = Math.Max(1, pagina);
            porPagina = Math.Clamp(porPagina, 1, 100);

            var query = AplicarFiltros(filtro);

            var totalRegistros = await query.CountAsync();

            var registros = await query
                .OrderByDescending(a => a.Fecha)
                .Skip((pagina - 1) * porPagina)
                .Take(porPagina)
                .Select(a => new AuditoriaDto
                {
                    Id = a.Id,
                    Usuario = a.Usuario,
                    Rol = a.Rol,
                    Accion = a.Accion,
                    Modulo = a.Modulo,
                    Detalle = a.Detalle,
                    RegistroId = a.RegistroId,
                    Fecha = a.Fecha,
                    IpAddress = a.IpAddress,
                    Resultado = a.Resultado
                })
                .ToListAsync();

            return new AuditoriaPaginadaDto
            {
                Registros = registros,
                TotalRegistros = totalRegistros,
                Pagina = pagina,
                PorPagina = porPagina,
                TotalPaginas = (int)Math.Ceiling(
                    totalRegistros / (double)porPagina)
            };
        }

        // Aplicar filtros comunes
        private IQueryable<global::SistemaFlota.Auditoria>
            AplicarFiltros(AuditoriaFiltroDto filtro)
        {
            var query = _context.Auditorias.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(filtro.Usuario))
            {
                query = query.Where(a =>
                    a.Usuario.Contains(filtro.Usuario));
            }

            if (!string.IsNullOrWhiteSpace(filtro.Rol))
            {
                query = query.Where(a =>
                    a.Rol == filtro.Rol);
            }

            if (!string.IsNullOrWhiteSpace(filtro.Accion))
            {
                query = query.Where(a =>
                    a.Accion == filtro.Accion);
            }

            if (!string.IsNullOrWhiteSpace(filtro.Modulo))
            {
                query = query.Where(a =>
                    a.Modulo == filtro.Modulo);
            }

            if (!string.IsNullOrWhiteSpace(filtro.Resultado))
            {
                query = query.Where(a =>
                    a.Resultado == filtro.Resultado);
            }

            if (filtro.Desde.HasValue)
            {
                var desde = filtro.Desde.Value;
                query = query.Where(a => a.Fecha >= desde);
            }

            if (filtro.Hasta.HasValue)
            {
                var hastaExclusivo = filtro.Hasta.Value.Date.AddDays(1);
                query = query.Where(a =>
                    a.Fecha < hastaExclusivo);
            }

            return query;
        }
        public async Task<object> ObtenerEstadisticasAsync()
        {
            var hoy = DateTime.Today;

            var porModulo = await _context.Auditorias
                .AsNoTracking()
                .GroupBy(a => a.Modulo)
                .Select(g => new
                {
                    modulo = g.Key,
                    total = g.Count()
                })
                .ToListAsync();

            var porAccion = await _context.Auditorias
                .AsNoTracking()
                .GroupBy(a => a.Accion)
                .Select(g => new
                {
                    accion = g.Key,
                    total = g.Count()
                })
                .ToListAsync();

            var porUsuario = await _context.Auditorias
                .AsNoTracking()
                .GroupBy(a => a.Usuario)
                .Select(g => new
                {
                    usuario = g.Key,
                    total = g.Count()
                })
                .OrderByDescending(g => g.total)
                .Take(10)
                .ToListAsync();

            var actividadHoy = await _context.Auditorias
                .CountAsync(a => a.Fecha >= hoy);

            var fallidosHoy = await _context.Auditorias
                .CountAsync(a =>
                    a.Fecha >= hoy && a.Resultado == "Fallido");

            var totalRegistros = await _context.Auditorias.CountAsync();

            return new
            {
                porModulo,
                porAccion,
                porUsuario,
                actividadHoy,
                fallidosHoy,
                totalRegistros
            };
        }

        public async Task<int> LimpiarAsync(int dias = 90)
        {
            if (dias < 1)
                throw new ArgumentOutOfRangeException(
                    nameof(dias),
                    "Los días deben ser mayores que cero.");

            var fechaLimite = DateTime.Now.AddDays(-dias);

            var registros = await _context.Auditorias
                .Where(a => a.Fecha < fechaLimite)
                .ToListAsync();

            if (registros.Count == 0)
                return 0;

            _context.Auditorias.RemoveRange(registros);

            await _context.SaveChangesAsync();

            return registros.Count;
        }
    }
}
