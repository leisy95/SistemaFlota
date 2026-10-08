using Microsoft.EntityFrameworkCore;
using SistemaFlota.DTOs.HistorialVersionOdo;

namespace SistemaFlota.Services.HistorialVersionOdo
{
    public class PedidoCompraService : IPedidoCompraService
    {
        private readonly AppDbContext _context;

        public PedidoCompraService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<PaginacionDto<PedidoCompraDto>> ObtenerAsync(
            int pagina = 1,
            int porPagina = 20,
            string? buscar = null,
            string? prioridad = null,
            string? estado = null,
            DateTime? fechaDesde = null,
            DateTime? fechaHasta = null)
        {
            if (pagina < 1)
                pagina = 1;

            if (porPagina < 1)
                porPagina = 20;

            if (porPagina > 100)
                porPagina = 100;

            var query = _context.PedidosCompra
                .AsNoTracking()
                .AsQueryable();

            // Búsqueda general
            if (!string.IsNullOrWhiteSpace(buscar))
            {
                buscar = buscar.Trim();

                query = query.Where(x =>
                    x.ReferenciaOrden.Contains(buscar) ||
                    x.Proveedor.Contains(buscar) ||
                    x.Comprador.Contains(buscar) ||
                    x.Estado.Contains(buscar));
            }

            // Filtro por prioridad
            if (!string.IsNullOrWhiteSpace(prioridad))
            {
                query = query.Where(x => x.Prioridad == prioridad);
            }

            // Filtro por estado
            if (!string.IsNullOrWhiteSpace(estado))
            {
                query = query.Where(x => x.Estado == estado);
            }

            // Filtro fecha desde
            if (fechaDesde.HasValue)
            {
                query = query.Where(x =>
                    x.FechaLimiteOrden >= fechaDesde.Value);
            }

            // Filtro fecha hasta
            if (fechaHasta.HasValue)
            {
                var fechaFinal = fechaHasta.Value.Date.AddDays(1);

                query = query.Where(x =>
                    x.FechaLimiteOrden < fechaFinal);
            }

            var totalRegistros = await query.CountAsync();

            var totalPaginas = (int)Math.Ceiling(
                totalRegistros / (double)porPagina);

            var datos = await query
                .OrderByDescending(x => x.FechaLimiteOrden)
                .Skip((pagina - 1) * porPagina)
                .Take(porPagina)
                .Select(x => new PedidoCompraDto
                {
                    Id = x.Id,
                    Prioridad = x.Prioridad,
                    ReferenciaOrden = x.ReferenciaOrden,
                    Proveedor = x.Proveedor,
                    Comprador = x.Comprador,
                    FechaLimiteOrden = x.FechaLimiteOrden,
                    Total = x.Total,
                    Estado = x.Estado
                })
                .ToListAsync();

            return new PaginacionDto<PedidoCompraDto>
            {
                Datos = datos,
                Pagina = pagina,
                PorPagina = porPagina,
                TotalRegistros = totalRegistros,
                TotalPaginas = totalPaginas
            };
        }

        public async Task<PedidoCompraDto?> ObtenerPorIdAsync(int id)
        {
            return await _context.PedidosCompra
                .AsNoTracking()
                .Where(x => x.Id == id)
                .Select(x => new PedidoCompraDto
                {
                    Id = x.Id,
                    Prioridad = x.Prioridad,
                    ReferenciaOrden = x.ReferenciaOrden,
                    Proveedor = x.Proveedor,
                    Comprador = x.Comprador,
                    FechaLimiteOrden = x.FechaLimiteOrden,
                    Total = x.Total,
                    Estado = x.Estado
                })
                .FirstOrDefaultAsync();
        }
    }
}