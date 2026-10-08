using Microsoft.EntityFrameworkCore;
using SistemaFlota.DTOs.HistorialVersionOdo;

namespace SistemaFlota.Services.HistorialVersionOdo
{
    public class MovimientoProductoService : IMovimientoProductoService
    {
        private readonly AppDbContext _context;

        public MovimientoProductoService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<PaginacionDto<MovimientoProductoDto>> ObtenerAsync(
            int pagina = 1,
            int porPagina = 50,
            string? buscar = null,
            string? producto = null,
            string? proveedor = null,
            string? estado = null,
            string? unidadMedida = null,
            DateTime? fechaDesde = null,
            DateTime? fechaHasta = null)
        {
            if (pagina < 1)
                pagina = 1;

            if (porPagina < 1)
                porPagina = 50;

            if (porPagina > 100)
                porPagina = 100;

            var query = _context.MovimientosProducto
                .AsNoTracking()
                .AsQueryable();

            // Búsqueda general
            if (!string.IsNullOrWhiteSpace(buscar))
            {
                buscar = buscar.Trim();

                query = query.Where(x =>
                    x.Referencia.Contains(buscar) ||
                    x.Producto.Contains(buscar) ||
                    x.Proveedor.Contains(buscar) ||
                    x.Estado.Contains(buscar));
            }

            // Filtro producto
            if (!string.IsNullOrWhiteSpace(producto))
            {
                query = query.Where(x =>
                    x.Producto == producto);
            }

            // Filtro proveedor
            if (!string.IsNullOrWhiteSpace(proveedor))
            {
                query = query.Where(x =>
                    x.Proveedor == proveedor);
            }

            // Filtro estado
            if (!string.IsNullOrWhiteSpace(estado))
            {
                query = query.Where(x =>
                    x.Estado == estado);
            }

            // Filtro unidad de medida
            if (!string.IsNullOrWhiteSpace(unidadMedida))
            {
                query = query.Where(x =>
                    x.UnidadMedida == unidadMedida);
            }

            // Filtro fecha desde
            if (fechaDesde.HasValue)
            {
                query = query.Where(x =>
                    x.Fecha >= fechaDesde.Value);
            }

            // Filtro fecha hasta
            if (fechaHasta.HasValue)
            {
                var fechaFinal = fechaHasta.Value.Date.AddDays(1);

                query = query.Where(x =>
                    x.Fecha < fechaFinal);
            }

            var totalRegistros = await query.CountAsync();

            var totalPaginas = (int)Math.Ceiling(
                totalRegistros / (double)porPagina);

            var datos = await query
                .OrderByDescending(x => x.Fecha)
                .Skip((pagina - 1) * porPagina)
                .Take(porPagina)
                .Select(x => new MovimientoProductoDto
                {
                    Id = x.Id,
                    Fecha = x.Fecha,
                    Referencia = x.Referencia,
                    Producto = x.Producto,
                    Proveedor = x.Proveedor,
                    Cantidad = x.Cantidad,
                    UnidadMedida = x.UnidadMedida,
                    Estado = x.Estado
                })
                .ToListAsync();

            return new PaginacionDto<MovimientoProductoDto>
            {
                Datos = datos,
                Pagina = pagina,
                PorPagina = porPagina,
                TotalRegistros = totalRegistros,
                TotalPaginas = totalPaginas
            };
        }

        public async Task<MovimientoProductoDto?> ObtenerPorIdAsync(int id)
        {
            return await _context.MovimientosProducto
                .AsNoTracking()
                .Where(x => x.Id == id)
                .Select(x => new MovimientoProductoDto
                {
                    Id = x.Id,
                    Fecha = x.Fecha,
                    Referencia = x.Referencia,
                    Producto = x.Producto,
                    Proveedor = x.Proveedor,
                    Cantidad = x.Cantidad,
                    UnidadMedida = x.UnidadMedida,
                    Estado = x.Estado
                })
                .FirstOrDefaultAsync();
        }
    }
}