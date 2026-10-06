using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using SistemaFlota.DTOs.ComprasNoFormalizadas.Inventario;
using SistemaFlota.Models.ComprasNoFormalizadas.RecepcionMercancias;
using SistemaFlota.Services.Auth;

namespace SistemaFlota.Services.ComprasNoFormalizadas.Inventario
{
    public class InventarioNoFormalizadoService : IInventarioNoFormalizadoService
    {
        private readonly AppDbContext _context;
        private readonly ICurrentUserService _currentUser;

        public InventarioNoFormalizadoService(
            AppDbContext context,
            ICurrentUserService currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task ProcesarRecepcionAsync(int recepcionId)
        {
            var recepcion = await ObtenerRecepcionAsync(recepcionId);

            var detallesPendientes = recepcion.Detalles
                .Where(d => !d.ProcesadoInventario)
                .ToList();

            if (!detallesPendientes.Any())
            {
                throw new Exception(
                    "No existen materiales pendientes por ingresar al inventario."
                );
            }

            var grupos = detallesPendientes
                .GroupBy(d => new
                {
                    MaterialId = d.OrdenCompraDetalleNoFormalizada!.MaterialNoFormalizadoId,
                    Color = d.OrdenCompraDetalleNoFormalizada.Color
                })
                .ToList();

            foreach (var grupo in grupos)
            {
                var detallesGrupo = grupo.ToList();

                var materialId = grupo.Key.MaterialId;
                var color = grupo.Key.Color;

                var inventario = await _context.InventariosNoFormalizados
                    .FirstOrDefaultAsync(i =>
                        i.MaterialId == materialId &&
                        i.Color == color);

                var cantidadTotal = detallesGrupo.Sum(d => d.CantidadRecibida);

                var valorTotalCompra = detallesGrupo.Sum(d =>
                    d.CantidadRecibida *
                    d.OrdenCompraDetalleNoFormalizada!.CostoKg);

                if (inventario == null)
                {
                    var costoPromedio = cantidadTotal > 0
                        ? valorTotalCompra / cantidadTotal
                        : 0;

                    inventario =
                        new Models.ComprasNoFormalizadas.Inventario.InventarioNoFormalizado
                        {
                            MaterialId = materialId,
                            Color = color,
                            StockActual = cantidadTotal,
                            CostoPromedio = costoPromedio,
                            ValorInventario = valorTotalCompra,
                            FechaCreacion = DateTime.Now,
                            FechaActualizacion = DateTime.Now
                        };

                    _context.InventariosNoFormalizados.Add(inventario);
                }
                else
                {
                    var stockAnterior = inventario.StockActual;
                    var costoAnterior = inventario.CostoPromedio;

                    var nuevoStock = stockAnterior + cantidadTotal;

                    var nuevoCostoPromedio = nuevoStock > 0
                        ? ((stockAnterior * costoAnterior) + valorTotalCompra)
                          / nuevoStock
                        : 0;

                    inventario.StockActual = nuevoStock;
                    inventario.CostoPromedio = nuevoCostoPromedio;
                    inventario.ValorInventario =
                        nuevoStock * nuevoCostoPromedio;
                    inventario.FechaActualizacion = DateTime.Now;
                }

                foreach (var detalle in detallesGrupo)
                {
                    detalle.ProcesadoInventario = true;
                }
            }

            await _context.SaveChangesAsync();
        }

        private async Task<RecepcionMercanciaNoFormalizada> ObtenerRecepcionAsync(
            int recepcionId)
        {
            var recepcion =
                await _context.RecepcionesMercanciasNoFormalizadas
                    .Include(r => r.Detalles)
                        .ThenInclude(d => d.OrdenCompraDetalleNoFormalizada)
                            .ThenInclude(o => o.MaterialNoFormalizado)
                    .FirstOrDefaultAsync(r => r.Id == recepcionId);

            if (recepcion == null)
                throw new Exception("La recepción no existe.");

            return recepcion;
        }

        public async Task<InventarioNoFormalizadoPaginadoDto> ObtenerAsync(
            string? search,
            int? proveedorId,
            string? categoria,
            string? color,
            int page,
            int pageSize,
            bool puedeVerDatosNumericos)
        {
            var query = _context.InventariosNoFormalizados
                .AsNoTracking()
                .Include(i => i.Material)
                    .ThenInclude(m => m.ProveedorNoFormalizado)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();

                query = query.Where(i =>
                    i.Material!.Codigo.Contains(search) ||
                    i.Material.NombreMaterial.Contains(search) ||
                    (i.Material.DescripcionCompra != null &&
                     i.Material.DescripcionCompra.Contains(search)) ||
                    i.Material.ProveedorNoFormalizado!.Nombre.Contains(search) ||
                    i.Color.Contains(search) ||
                    i.Material.Categoria.Contains(search) ||
                    i.Material.Densidad.Contains(search) ||
                    (i.Material.TipoProduccion != null &&
                     i.Material.TipoProduccion.Contains(search)));
            }

            if (proveedorId.HasValue)
            {
                query = query.Where(i =>
                    i.Material!.IdProveedorNoFormalizado == proveedorId.Value);
            }

            if (!string.IsNullOrWhiteSpace(categoria))
            {
                query = query.Where(i =>
                    i.Material!.Categoria == categoria);
            }

            if (!string.IsNullOrWhiteSpace(color))
            {
                query = query.Where(i =>
                    i.Color == color);
            }

            var total = await query.CountAsync();

            decimal? totalKg = null;
            decimal? totalValorInventario = null;

            if (puedeVerDatosNumericos)
            {
                totalKg = await query.SumAsync(i => i.StockActual);
                totalValorInventario =
                    await query.SumAsync(i => i.ValorInventario);
            }

            var items = await query
                .OrderBy(i => i.Material!.NombreMaterial)
                .ThenBy(i => i.Color)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(i => new InventarioNoFormalizadoDto
                {
                    Id = i.Id,
                    MaterialId = i.MaterialId,
                    Material = i.Material!.NombreMaterial ?? "",
                    Proveedor = i.Material.ProveedorNoFormalizado!.Nombre,
                    Categoria = i.Material.Categoria,
                    Color = i.Color,
                    Densidad = i.Material.Densidad,

                    StockActual = puedeVerDatosNumericos
                        ? i.StockActual
                        : null,

                    CantidadComprometida = puedeVerDatosNumericos
                        ? _context.OrdenesTrasladoNoFormalizadas
                            .Where(o =>
                                o.Estado == "Pendiente" ||
                                o.Estado == "Verificando")
                            .SelectMany(o => o.Detalles)
                            .Where(d =>
                                d.MaterialNoFormalizadoId == i.MaterialId &&
                                d.Color == i.Color)
                            .Sum(d => (decimal?)d.CantidadKg) ?? 0m
                        : null,

                    StockDisponible = puedeVerDatosNumericos
                        ? Math.Max(
                            i.StockActual -
                            (
                                _context.OrdenesTrasladoNoFormalizadas
                                    .Where(o =>
                                        o.Estado == "Pendiente" ||
                                        o.Estado == "Verificando")
                                    .SelectMany(o => o.Detalles)
                                    .Where(d =>
                                        d.MaterialNoFormalizadoId == i.MaterialId &&
                                        d.Color == i.Color)
                                    .Sum(d => (decimal?)d.CantidadKg) ?? 0m
                            ),
                            0m)
                        : null,

                    CostoPromedio = puedeVerDatosNumericos
                        ? i.CostoPromedio
                        : null,

                    ValorInventario = puedeVerDatosNumericos
                        ? i.ValorInventario
                        : null
                })
                .ToListAsync();

            return new InventarioNoFormalizadoPaginadoDto
            {
                Items = items,
                Total = total,
                Pagina = page,
                PageSize = pageSize,
                TotalKg = totalKg,
                TotalValorInventario = totalValorInventario
            };
        }

        public async Task<List<ProveedorFiltroNoFormalizadoDto>>
            ObtenerProveedoresInventarioAsync()
        {
            return await _context.InventariosNoFormalizados
                .AsNoTracking()
                .Include(i => i.Material)
                    .ThenInclude(m => m.ProveedorNoFormalizado)
                .Select(i => new ProveedorFiltroNoFormalizadoDto
                {
                    Id = i.Material!.IdProveedorNoFormalizado,
                    Nombre = i.Material.ProveedorNoFormalizado!.Nombre
                })
                .Distinct()
                .OrderBy(p => p.Nombre)
                .ToListAsync();
        }

        public async Task<List<string>> ObtenerCategoriasInventarioAsync()
        {
            return await _context.InventariosNoFormalizados
                .AsNoTracking()
                .Include(i => i.Material)
                .Select(i => i.Material!.Categoria)
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync();
        }

        public async Task<byte[]> ExportarExcelAsync(
            string? search,
            int? proveedorId,
            string? categoria,
            string? color,
            bool puedeVerDatosNumericos)
        {
            var query = _context.InventariosNoFormalizados
                .AsNoTracking()
                .Include(i => i.Material)
                    .ThenInclude(m => m.ProveedorNoFormalizado)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();

                query = query.Where(i =>
                    i.Material!.Codigo.Contains(search) ||
                    i.Material.NombreMaterial.Contains(search) ||
                    (i.Material.DescripcionCompra != null &&
                     i.Material.DescripcionCompra.Contains(search)) ||
                    i.Material.ProveedorNoFormalizado!.Nombre.Contains(search) ||
                    i.Color.Contains(search) ||
                    i.Material.Categoria.Contains(search) ||
                    i.Material.Densidad.Contains(search) ||
                    (i.Material.TipoProduccion != null &&
                     i.Material.TipoProduccion.Contains(search)));
            }

            if (proveedorId.HasValue)
            {
                query = query.Where(i =>
                    i.Material!.IdProveedorNoFormalizado == proveedorId.Value);
            }

            if (!string.IsNullOrWhiteSpace(categoria))
            {
                query = query.Where(i =>
                    i.Material!.Categoria == categoria);
            }

            if (!string.IsNullOrWhiteSpace(color))
            {
                query = query.Where(i =>
                    i.Color == color);
            }

            var inventario = await query
                .OrderBy(i => i.Material!.NombreMaterial)
                .ThenBy(i => i.Color)
                .ToListAsync();

            var empresa = await _context.ConfiguracionEmpresa
                .FirstOrDefaultAsync();

            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Id == _currentUser.IdUsuario);

            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("Reporte Inventario");

            var ultimaColumna =
                puedeVerDatosNumericos ? "H" : "E";

            var cantidadColumnas =
                puedeVerDatosNumericos ? 8 : 5;

            ws.Range($"A1:{ultimaColumna}1").Merge();
            ws.Cell("A1").Value =
                empresa?.NombreEmpresa ?? "EMPRESA";
            ws.Cell("A1").Style.Font.Bold = true;
            ws.Cell("A1").Style.Font.FontSize = 18;
            ws.Cell("A1").Style.Alignment.Horizontal =
                XLAlignmentHorizontalValues.Center;

            ws.Range($"A2:{ultimaColumna}2").Merge();
            ws.Cell("A2").Value = "REPORTE DE INVENTARIO";
            ws.Cell("A2").Style.Font.Bold = true;
            ws.Cell("A2").Style.Font.FontSize = 14;
            ws.Cell("A2").Style.Alignment.Horizontal =
                XLAlignmentHorizontalValues.Center;

            ws.Range($"A3:{ultimaColumna}3").Merge();
            ws.Cell("A3").Value =
                $"Fecha exportación: {DateTime.Now:dd/MM/yyyy HH:mm}";

            ws.Range($"A4:{ultimaColumna}4").Merge();
            ws.Cell("A4").Value =
                $"Usuario exportación: {usuario?.Username ?? "Sistema"}";

            ws.Range($"A5:{ultimaColumna}5").Merge();
            ws.Cell("A5").Value =
                $"Filtros aplicados: Búsqueda={search ?? "Todos"} | " +
                $"Categoría={categoria ?? "Todas"} | " +
                $"Color={color ?? "Todos"}";

            var filaInicio = 7;

            var headers = puedeVerDatosNumericos
                ? new[]
                {
                    "Material",
                    "Proveedor",
                    "Categoría",
                    "Color",
                    "Densidad",
                    "Stock Actual",
                    "Costo Promedio",
                    "Valor Inventario"
                }
                : new[]
                {
                    "Material",
                    "Proveedor",
                    "Categoría",
                    "Color",
                    "Densidad"
                };

            for (int i = 0; i < headers.Length; i++)
                ws.Cell(filaInicio, i + 1).Value = headers[i];

            var header = ws.Range(
                filaInicio,
                1,
                filaInicio,
                cantidadColumnas);

            header.Style.Fill.BackgroundColor =
                XLColor.FromHtml("#1F4E78");

            header.Style.Font.FontColor = XLColor.White;
            header.Style.Font.Bold = true;
            header.Style.Alignment.Horizontal =
                XLAlignmentHorizontalValues.Center;

            int fila = filaInicio + 1;
            decimal totalStock = 0;
            decimal totalValor = 0;

            foreach (var item in inventario)
            {
                ws.Cell(fila, 1).Value =
                    item.Material?.NombreMaterial;

                ws.Cell(fila, 2).Value =
                    item.Material?.ProveedorNoFormalizado?.Nombre;

                ws.Cell(fila, 3).Value =
                    item.Material?.Categoria;

                ws.Cell(fila, 4).Value =
                    item.Color;

                ws.Cell(fila, 5).Value =
                    item.Material?.Densidad;

                if (puedeVerDatosNumericos)
                {
                    ws.Cell(fila, 6).Value =
                        item.StockActual;

                    ws.Cell(fila, 7).Value =
                        item.CostoPromedio;

                    ws.Cell(fila, 8).Value =
                        item.ValorInventario;

                    totalStock += item.StockActual;
                    totalValor += item.ValorInventario;
                }

                fila++;
            }

            if (fila > filaInicio + 1)
            {
                var tabla = ws.Range(
                    filaInicio,
                    1,
                    fila - 1,
                    cantidadColumnas);

                tabla.CreateTable();
            }

            if (puedeVerDatosNumericos)
            {
                ws.Cell(fila + 1, 5).Value = "TOTAL";
                ws.Cell(fila + 1, 5).Style.Font.Bold = true;

                ws.Cell(fila + 1, 6).Value = totalStock;
                ws.Cell(fila + 1, 6).Style.Font.Bold = true;

                ws.Cell(fila + 1, 8).Value = totalValor;
                ws.Cell(fila + 1, 8).Style.Font.Bold = true;

                ws.Column(6).Style.NumberFormat.Format =
                    "#,##0.00";

                ws.Column(7).Style.NumberFormat.Format =
                    "$ #,##0";

                ws.Column(8).Style.NumberFormat.Format =
                    "$ #,##0";
            }

            ws.Columns().AdjustToContents();
            ws.SheetView.FreezeRows(filaInicio);

            using var stream = new MemoryStream();

            workbook.SaveAs(stream);

            return stream.ToArray();
        }
    }
}