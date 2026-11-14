using Microsoft.EntityFrameworkCore;
using SistemaFlota.DTOs.InventarioBodega.HistorialMovimientos;

namespace SistemaFlota.Services.InventarioBodega.HistorialMovimientos
{
    public class HistorialInventarioBodegaService :
       IHistorialInventarioBodegaService
    {
        private readonly AppDbContext _context;

        private readonly string[] _modulosPermitidos =
        {
            "InventarioBodega",
            "SalidasBodega"
        };

        public HistorialInventarioBodegaService(AppDbContext context)
        {
            _context = context;
        }

        private IQueryable<Auditoria> AplicarFiltros(
            DateTime? desde,
            DateTime? hasta,
            string? modulo,
            string? accion,
            string? usuario,
            string? buscar)
        {
            var query = _context.Auditorias
                .AsNoTracking()
                .Where(x => _modulosPermitidos.Contains(x.Modulo));

            if (desde.HasValue)
                query = query.Where(x => x.Fecha >= desde.Value.Date);

            if (hasta.HasValue)
            {
                var limite = hasta.Value.Date.AddDays(1);
                query = query.Where(x => x.Fecha < limite);
            }

            if (!string.IsNullOrWhiteSpace(modulo))
                query = query.Where(x => x.Modulo == modulo);

            if (!string.IsNullOrWhiteSpace(accion))
                query = query.Where(x => x.Accion == accion);

            if (!string.IsNullOrWhiteSpace(usuario))
                query = query.Where(x => x.Usuario == usuario);

            if (!string.IsNullOrWhiteSpace(buscar))
            {
                var texto = buscar.Trim();

                query = query.Where(x =>
                    x.Usuario.Contains(texto) ||
                    x.Modulo.Contains(texto) ||
                    x.Accion.Contains(texto) ||
                    (x.Detalle != null && x.Detalle.Contains(texto)) ||
                    (x.RegistroId.HasValue &&
                     x.RegistroId.Value.ToString().Contains(texto)));
            }

            return query;
        }

        private static MovimientoInventarioBodegaDto Mapear(Auditoria x)
        {
            return new MovimientoInventarioBodegaDto
            {
                Id = x.Id,
                Fecha = x.Fecha,
                Usuario = x.Usuario,
                Rol = x.Rol,
                Modulo = x.Modulo,
                Accion = x.Accion,
                Detalle = x.Detalle,
                RegistroId = x.RegistroId,
                IpAddress = x.IpAddress,
                Resultado = x.Resultado
            };
        }

        public async Task<MovimientoInventarioBodegaPaginadoDto> ObtenerAsync(
            DateTime? desde,
            DateTime? hasta,
            string? modulo,
            string? accion,
            string? usuario,
            string? buscar,
            int pagina,
            int porPagina)
        {
            pagina = Math.Max(1, pagina);
            porPagina = Math.Clamp(porPagina, 1, 100);

            var query = AplicarFiltros(
                desde, hasta, modulo, accion, usuario, buscar);

            var total = await query.CountAsync();

            var movimientos = await query
                .OrderByDescending(x => x.Fecha)
                .ThenByDescending(x => x.Id)
                .Skip((pagina - 1) * porPagina)
                .Take(porPagina)
                .Select(x => new MovimientoInventarioBodegaDto
                {
                    Id = x.Id,
                    Fecha = x.Fecha,
                    Usuario = x.Usuario,
                    Rol = x.Rol,
                    Modulo = x.Modulo,
                    Accion = x.Accion,
                    Detalle = x.Detalle,
                    RegistroId = x.RegistroId,
                    IpAddress = x.IpAddress,
                    Resultado = x.Resultado
                })
                .ToListAsync();

            return new MovimientoInventarioBodegaPaginadoDto
            {
                Movimientos = movimientos,
                TotalRegistros = total,
                Pagina = pagina,
                PorPagina = porPagina,
                TotalPaginas = (int)Math.Ceiling(
                    total / (double)porPagina)
            };
        }

        public async Task<MovimientoInventarioBodegaResumenDto> ObtenerResumenAsync(
            DateTime? desde,
            DateTime? hasta,
            string? modulo,
            string? accion,
            string? usuario,
            string? buscar)
        {
            var query = AplicarFiltros(
                desde, hasta, modulo, accion, usuario, buscar);

            var datos = await query
                .GroupBy(x => 1)
                .Select(g => new MovimientoInventarioBodegaResumenDto
                {
                    TotalMovimientos = g.Count(),

                    TotalCreaciones = g.Count(x =>
                        x.Accion == "Crear"),

                    TotalActualizaciones = g.Count(x =>
                        x.Accion == "Editar" ||
                        x.Accion == "Actualizar"),

                    TotalEliminaciones = g.Count(x =>
                        x.Accion == "Eliminar"),

                    TotalConfirmaciones = g.Count(x =>
                        x.Accion == "Confirmar"),

                    TotalCancelaciones = g.Count(x =>
                        x.Accion == "Cancelar")
                })
                .FirstOrDefaultAsync();

            return datos ?? new MovimientoInventarioBodegaResumenDto();
        }

        public async Task<MovimientoInventarioBodegaOpcionesDto> ObtenerOpcionesAsync()
        {
            var query = _context.Auditorias
                .AsNoTracking()
                .Where(x => _modulosPermitidos.Contains(x.Modulo));

            var modulos = await query
                .Select(x => x.Modulo)
                .Distinct()
                .OrderBy(x => x)
                .ToListAsync();

            var usuarios = await query
                .Where(x => x.Usuario != null && x.Usuario != "")
                .Select(x => x.Usuario)
                .Distinct()
                .OrderBy(x => x)
                .ToListAsync();

            var acciones = await query
                .Select(x => x.Accion)
                .Distinct()
                .OrderBy(x => x)
                .ToListAsync();

            return new MovimientoInventarioBodegaOpcionesDto
            {
                Modulos = modulos,
                Usuarios = usuarios,
                Acciones = acciones
            };
        }

        public async Task<MovimientoInventarioBodegaDto?> ObtenerPorIdAsync(int id)
        {
            var auditoria = await _context.Auditorias
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    _modulosPermitidos.Contains(x.Modulo));

            return auditoria == null ? null : Mapear(auditoria);
        }
    }
}
