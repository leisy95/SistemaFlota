
using Microsoft.EntityFrameworkCore;
using SistemaFlota.DTOs.Costos.HistorialMovimientos;

namespace SistemaFlota.Services.Costos.HistorialMovimientos
{
    public class HistorialMovimientosService : IHistorialMovimientosService
    {
        private readonly AppDbContext _context;

        public HistorialMovimientosService(AppDbContext context)
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

            var modulosPermitidos = new[]
            {
                "OrdenCompra",
                "Inventario",
                "OrdenTraslado",
                "Materiales",
                "Proveedores",
                "RecepcionMercancia"
            };

            var query = _context.Auditorias
                .AsNoTracking()
                .Where(x => modulosPermitidos.Contains(x.Modulo));

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
                buscar = buscar.Trim();

                query = query.Where(x =>
                    x.Usuario.Contains(buscar) ||
                    x.Modulo.Contains(buscar) ||
                    x.Accion.Contains(buscar) ||
                    (x.Detalle != null && x.Detalle.Contains(buscar)) ||
                    (x.RegistroId.HasValue &&
                     x.RegistroId.Value.ToString().Contains(buscar)));
            }

            return query;
        }

        private static MovimientoHistorialDto Mapear(Auditoria x)
        {
            return new MovimientoHistorialDto
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


        public async Task<MovimientoHistorialPaginadoDto> ObtenerAsync(
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
            porPagina = Math.Clamp(porPagina, 1, 10);

            var query = AplicarFiltros(
                desde, hasta, modulo, accion, usuario, buscar);

            var total = await query.CountAsync();

            var movimientos = await query
                .OrderByDescending(x => x.Fecha)
                .ThenByDescending(x => x.Id)
                .Skip((pagina - 1) * porPagina)
                .Take(porPagina)
                .Select(x => new MovimientoHistorialDto
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

            return new MovimientoHistorialPaginadoDto
            {
                Movimientos = movimientos,
                TotalRegistros = total,
                Pagina = pagina,
                PorPagina = porPagina,
                TotalPaginas = (int)Math.Ceiling(
                    total / (double)porPagina)
            };
        }


        public async Task<MovimientoHistorialResumenDto> ObtenerResumenAsync(
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
                .Select(g => new MovimientoHistorialResumenDto
                {
                    TotalMovimientos = g.Count(),
                    TotalCreaciones = g.Count(x => x.Accion == "Crear"),
                    TotalActualizaciones = g.Count(x =>
                        x.Accion == "Editar" ||
                        x.Accion == "Actualizar"),
                    TotalEliminaciones = g.Count(x =>
                        x.Accion == "Eliminar")
                })
                .FirstOrDefaultAsync();

            return datos ?? new MovimientoHistorialResumenDto();
        }

        // Obtiene las opciones de todos los registros,
        // sin depender de la página actual.
        public async Task<MovimientoHistorialOpcionesDto> ObtenerOpcionesAsync()
        {
            var modulosPermitidos = new[]
            {
                "OrdenCompra",
                "Inventario",
                "OrdenTraslado",
                "Materiales",
                "Proveedores",
                "RecepcionMercancia"
            };

            var modulos = await _context.Auditorias
                .AsNoTracking()
                .Where(x => modulosPermitidos.Contains(x.Modulo))
                .Select(x => x.Modulo)
                .Distinct()
                .OrderBy(x => x)
                .ToListAsync();

            var usuarios = await _context.Auditorias
                .AsNoTracking()
                .Where(x => x.Usuario != null && x.Usuario != "")
                .Select(x => x.Usuario)
                .Distinct()
                .OrderBy(x => x)
                .ToListAsync();

            return new MovimientoHistorialOpcionesDto
            {
                Modulos = modulos,
                Usuarios = usuarios
            };
        }

        public async Task<MovimientoHistorialDto?> ObtenerPorIdAsync(int id)
        {
            var auditoria = await _context.Auditorias
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);

            return auditoria == null ? null : Mapear(auditoria);
        }
    }
}
