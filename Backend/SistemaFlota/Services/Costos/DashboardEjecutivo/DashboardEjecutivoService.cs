
using Microsoft.EntityFrameworkCore;
using SistemaFlota.DTOs.Costos.DashboardEjecutivo;

namespace SistemaFlota.Services.Costos.DashboardEjecutivo
{
    public class DashboardEjecutivoService : IDashboardEjecutivoService
    {
        private readonly AppDbContext _context;

        public DashboardEjecutivoService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<DashboardEjecutivoDto> ObtenerDashboardAsync(
            int pagina = 1,
            int porPagina = 5,
            DateTime? fechaDesde = null,
            DateTime? fechaHasta = null,
            string? proveedor = null,
            string? estado = null,
            string? categoria = null,
            string? densidad = null)
        {
            pagina = Math.Max(1, pagina);
            porPagina = Math.Clamp(porPagina, 1, 100);

            proveedor = string.IsNullOrWhiteSpace(proveedor)
                ? null : proveedor.Trim();

            estado = string.IsNullOrWhiteSpace(estado)
                ? null : estado.Trim();

            categoria = string.IsNullOrWhiteSpace(categoria)
                ? null : categoria.Trim();

            densidad = string.IsNullOrWhiteSpace(densidad)
                ? null : densidad.Trim();

            if (fechaDesde.HasValue && fechaHasta.HasValue &&
                fechaDesde.Value.Date > fechaHasta.Value.Date)
            {
                throw new ArgumentException(
                    "La fecha inicial no puede ser posterior a la fecha final.");
            }

            var desde = fechaDesde?.Date;
            var hastaExclusiva = fechaHasta?.Date.AddDays(1);

            // INVENTARIO ACTUAL
            // El inventario es una fotografía actual; no se filtra por fecha.
            var inventariosQuery = _context.Inventarios
                .AsNoTracking()
                .Where(i => i.Material != null);

            if (proveedor != null)
            {
                inventariosQuery = inventariosQuery.Where(i =>
                    i.Material!.Proveedor != null &&
                    i.Material.Proveedor.Nombre == proveedor);
            }

            if (categoria != null)
            {
                inventariosQuery = inventariosQuery.Where(i =>
                    i.Material!.Categoria == categoria);
            }

            if (densidad != null)
            {
                inventariosQuery = inventariosQuery.Where(i =>
                    i.Material!.Densidad == densidad);
            }

            var inventarios = await inventariosQuery
                .Select(i => new
                {
                    i.StockActual,
                    i.ValorInventario,
                    i.CostoPromedio,

                    Proveedor = i.Material!.Proveedor != null
                        ? i.Material.Proveedor.Nombre
                        : "Sin proveedor",

                    Categoria = i.Material.Categoria,
                    Densidad = i.Material.Densidad
                })
                .ToListAsync();

            // ÓRDENES DE COMPRA
            var ordenesQuery = _context.OrdenesCompra
                .AsNoTracking()
                .Where(o => o.Activo);

            if (desde.HasValue)
            {
                ordenesQuery = ordenesQuery.Where(o =>
                    o.FechaOrden >= desde.Value);
            }

            if (hastaExclusiva.HasValue)
            {
                ordenesQuery = ordenesQuery.Where(o =>
                    o.FechaOrden < hastaExclusiva.Value);
            }

            if (proveedor != null)
            {
                ordenesQuery = ordenesQuery.Where(o =>
                    o.Proveedor != null &&
                    o.Proveedor.Nombre == proveedor);
            }

            if (estado != null)
            {
                ordenesQuery = ordenesQuery.Where(o =>
                    o.Estado == estado);
            }

            var ordenes = await ordenesQuery
                .Select(o => new
                {
                    o.Id,
                    o.Numero,
                    o.Estado,
                    o.FechaOrden,
                    o.TotalKg,
                    o.TotalBultos,
                    o.TotalPagar
                })
                .ToListAsync();

            // PROVEEDORES ACTIVOS
            var proveedoresQuery = _context.Proveedores
                .AsNoTracking()
                .Where(p => p.Activo);

            if (proveedor != null)
            {
                proveedoresQuery = proveedoresQuery.Where(p =>
                    p.Nombre == proveedor);
            }

            var proveedoresActivos = await proveedoresQuery.CountAsync();

            // TOTALES DEL INVENTARIO FILTRADO
            var totalKg = inventarios.Sum(i => i.StockActual);
            var totalValor = inventarios.Sum(i => i.ValorInventario);

            // TOTALES DE COMPRAS FILTRADAS
            var gastoCompras = ordenes.Sum(o => o.TotalPagar);

            // RECEPCIONES ASOCIADAS A LAS ÓRDENES FILTRADAS
            var recepciones = _context.RecepcionesMercancias
                .AsNoTracking()
                .Where(r =>
                    r.OrdenCompra != null &&
                    r.OrdenCompra.Activo);

            if (desde.HasValue)
            {
                recepciones = recepciones.Where(r =>
                    r.OrdenCompra!.FechaOrden >= desde.Value);
            }

            if (hastaExclusiva.HasValue)
            {
                recepciones = recepciones.Where(r =>
                    r.OrdenCompra!.FechaOrden < hastaExclusiva.Value);
            }

            if (proveedor != null)
            {
                recepciones = recepciones.Where(r =>
                    r.OrdenCompra!.Proveedor != null &&
                    r.OrdenCompra.Proveedor.Nombre == proveedor);
            }

            if (estado != null)
            {
                recepciones = recepciones.Where(r =>
                    r.OrdenCompra!.Estado == estado);
            }

            var totalRecepciones = await recepciones.CountAsync();

            // DETALLES DE RECEPCIÓN
            var detallesRecepcion = _context.RecepcionesMercanciaDetalle
                .AsNoTracking()
                .Where(d =>
                    d.RecepcionMercancia != null &&
                    d.RecepcionMercancia.OrdenCompra != null &&
                    d.RecepcionMercancia.OrdenCompra.Activo);

            if (desde.HasValue)
            {
                detallesRecepcion = detallesRecepcion.Where(d =>
                    d.RecepcionMercancia!.OrdenCompra!.FechaOrden
                    >= desde.Value);
            }

            if (hastaExclusiva.HasValue)
            {
                detallesRecepcion = detallesRecepcion.Where(d =>
                    d.RecepcionMercancia!.OrdenCompra!.FechaOrden
                    < hastaExclusiva.Value);
            }

            if (proveedor != null)
            {
                detallesRecepcion = detallesRecepcion.Where(d =>
                    d.RecepcionMercancia!.OrdenCompra!.Proveedor != null &&
                    d.RecepcionMercancia.OrdenCompra.Proveedor.Nombre
                    == proveedor);
            }

            if (estado != null)
            {
                detallesRecepcion = detallesRecepcion.Where(d =>
                    d.RecepcionMercancia!.OrdenCompra!.Estado == estado);
            }

            var kgRecibidos = await detallesRecepcion
                .SumAsync(d => (decimal?)d.CantidadRecibida) ?? 0m;

            var bultosRecibidos = await detallesRecepcion
                .SumAsync(d => (decimal?)d.BultosRecibidos) ?? 0m;

            var entregasPendientesInventario = await detallesRecepcion
                .CountAsync(d => !d.ProcesadoInventario);

            var recepcionesPendientes = await recepciones
                .Where(r => r.Detalles.Any(
                    d => !d.ProcesadoInventario))
                .CountAsync();

            // COMPRAS POR MES
            var comprasPorMes = ordenes
                .GroupBy(o => new
                {
                    o.FechaOrden.Year,
                    o.FechaOrden.Month
                })
                .Select(g => new CompraPorMesDto
                {
                    Mes = $"{g.Key.Year}-{g.Key.Month:D2}",
                    Valor = g.Sum(o => o.TotalPagar),
                    Kg = g.Sum(o => o.TotalKg)
                })
                .OrderBy(x => x.Mes)
                .ToList();

            // MATERIALES CON MAYOR VALOR DE COMPRA
            var materialesQuery = _context.OrdenesCompraDetalle
                .AsNoTracking()
                .Where(d =>
                    d.OrdenCompra != null &&
                    d.OrdenCompra.Activo &&
                    d.Material != null);

            if (desde.HasValue)
            {
                materialesQuery = materialesQuery.Where(d =>
                    d.OrdenCompra!.FechaOrden >= desde.Value);
            }

            if (hastaExclusiva.HasValue)
            {
                materialesQuery = materialesQuery.Where(d =>
                    d.OrdenCompra!.FechaOrden < hastaExclusiva.Value);
            }

            if (proveedor != null)
            {
                materialesQuery = materialesQuery.Where(d =>
                    d.OrdenCompra!.Proveedor != null &&
                    d.OrdenCompra.Proveedor.Nombre == proveedor);
            }

            if (estado != null)
            {
                materialesQuery = materialesQuery.Where(d =>
                    d.OrdenCompra!.Estado == estado);
            }

            if (categoria != null)
            {
                materialesQuery = materialesQuery.Where(d =>
                    d.Material!.Categoria == categoria);
            }

            if (densidad != null)
            {
                materialesQuery = materialesQuery.Where(d =>
                    d.Material!.Densidad == densidad);
            }

            var materialesAgrupados = materialesQuery
                .GroupBy(d => new
                {
                    d.MaterialId,
                    Material = d.Material!.NombreMaterial,
                    Proveedor = d.OrdenCompra!.Proveedor != null
                        ? d.OrdenCompra.Proveedor.Nombre
                        : "Sin proveedor"
                })
                .Select(g => new MaterialMayorCostoDto
                {
                    Material = g.Key.Material,
                    Proveedor = g.Key.Proveedor,
                    Kg = g.Sum(d => d.CantidadKg),
                    CostoKg = g.Sum(d => d.Subtotal) /
                        (g.Sum(d => d.CantidadKg) > 0
                            ? g.Sum(d => d.CantidadKg)
                            : 1),
                    Valor = g.Sum(d => d.Subtotal)
                });

            var totalMaterialesMayorCosto =
                await materialesAgrupados.CountAsync();

            var materialesMayorCosto = await materialesAgrupados
                .OrderByDescending(x => x.Valor)
                .ThenBy(x => x.Material)
                .Skip((pagina - 1) * porPagina)
                .Take(porPagina)
                .ToListAsync();

            // CONSTRUIR DASHBOARD
            return new DashboardEjecutivoDto
            {
                TotalKg = totalKg,
                TotalValor = totalValor,
                CostoPromedio = totalKg > 0
                    ? totalValor / totalKg
                    : 0,

                TotalProveedores = proveedoresActivos,
                TotalBultos = ordenes.Sum(o => o.TotalBultos),
                TotalOrdenes = ordenes.Count,

                OrdenesPendientes = ordenes.Count(o =>
                    string.Equals(
                        o.Estado,
                        "Pendiente",
                        StringComparison.OrdinalIgnoreCase)),

                GastoCompras = gastoCompras,
                TotalRecepciones = totalRecepciones,
                RecepcionesPendientes = recepcionesPendientes,
                KgRecibidos = kgRecibidos,
                BultosRecibidos = bultosRecibidos,
                EntregasPendientesInventario =
                    entregasPendientesInventario,

                ComprasPorMes = comprasPorMes,
                MaterialesMayorCosto = materialesMayorCosto,

                TotalMaterialesMayorCosto = totalMaterialesMayorCosto,
                PaginaMateriales = pagina,
                PorPaginaMateriales = porPagina,

                StockPorProveedor = inventarios
                    .GroupBy(i => i.Proveedor)
                    .Select(g => new StockProveedorDto
                    {
                        Proveedor = g.Key,
                        Kg = g.Sum(i => i.StockActual),
                        Valor = g.Sum(i => i.ValorInventario),
                        Bultos = 0,
                        Porcentaje = totalKg > 0
                            ? g.Sum(i => i.StockActual) / totalKg * 100
                            : 0
                    })
                    .OrderByDescending(p => p.Kg)
                    .ToList(),

                MaterialesPorCategoria = inventarios
                    .GroupBy(i => i.Categoria)
                    .Select(g => new CategoriaMaterialDto
                    {
                        Categoria = g.Key,
                        Cantidad = g.Count()
                    })
                    .ToList(),

                MaterialesPorDensidad = inventarios
                    .GroupBy(i => i.Densidad)
                    .Select(g => new DensidadMaterialDto
                    {
                        Densidad = g.Key,
                        Cantidad = g.Count()
                    })
                    .ToList(),

                ValorPorProveedor = inventarios
                    .GroupBy(i => i.Proveedor)
                    .Select(g => new ValorProveedorDto
                    {
                        Proveedor = g.Key,
                        Valor = g.Sum(i => i.ValorInventario)
                    })
                    .OrderByDescending(p => p.Valor)
                    .ToList()
            };
        }

        public async Task<FiltrosDashboardEjecutivoDto> ObtenerFiltrosAsync()
        {
            var estados = await _context.OrdenesCompra
                .AsNoTracking()
                .Where(o => o.Activo && o.Estado != null && o.Estado != "")
                .Select(o => o.Estado!)
                .Distinct()
                .OrderBy(e => e)
                .ToListAsync();

            var categorias = await _context.Inventarios
                .AsNoTracking()
                .Where(i => i.Material != null &&
                            i.Material.Categoria != null &&
                            i.Material.Categoria != "")
                .Select(i => i.Material!.Categoria!)
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync();

            var densidades = await _context.Inventarios
                .AsNoTracking()
                .Where(i => i.Material != null &&
                            i.Material.Densidad != null &&
                            i.Material.Densidad != "")
                .Select(i => i.Material!.Densidad!)
                .Distinct()
                .OrderBy(d => d)
                .ToListAsync();

            var proveedores = await _context.Proveedores
                .AsNoTracking()
                .Where(p => p.Activo)
                .Select(p => p.Nombre)
                .Distinct()
                .OrderBy(p => p)
                .ToListAsync();

            return new FiltrosDashboardEjecutivoDto
            {
                Estados = estados,
                Categorias = categorias,
                Densidades = densidades,
                Proveedores = proveedores
            };
        }
    }
}
