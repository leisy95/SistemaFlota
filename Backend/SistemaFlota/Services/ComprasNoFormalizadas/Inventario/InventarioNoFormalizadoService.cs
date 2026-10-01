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

            foreach (var detalle in recepcion.Detalles)
            {
                await ActualizarInventarioAsync(detalle);
            }

            await _context.SaveChangesAsync();
        }

        private async Task<RecepcionMercanciaNoFormalizada> ObtenerRecepcionAsync(int recepcionId)
        {
            var recepcion = await _context.RecepcionesMercanciasNoFormalizadas
                .Include(r => r.Detalles)
                    .ThenInclude(d => d.OrdenCompraDetalleNoFormalizada)
                        .ThenInclude(o => o.MaterialNoFormalizado)
                .FirstOrDefaultAsync(r => r.Id == recepcionId);

            if (recepcion == null)
                throw new Exception("La recepción no existe.");

            return recepcion;
        }

        private async Task ActualizarInventarioAsync(
            RecepcionMercanciaDetalleNoFormalizada detalle)
        {
            var ordenDetalle = detalle.OrdenCompraDetalleNoFormalizada;

            if (ordenDetalle == null)
                throw new Exception("El detalle de la orden de compra no existe.");

            var materialId = ordenDetalle.MaterialNoFormalizadoId;
            var color = ordenDetalle.Color;
            var cantidadCompra = detalle.CantidadRecibida;
            var costoCompra = ordenDetalle.CostoKg;

            var inventario = await _context.InventariosNoFormalizados
                .FirstOrDefaultAsync(i =>
                    i.MaterialId == materialId &&
                    i.Color == color);

            if (inventario == null)
            {
                inventario = new Models.ComprasNoFormalizadas.Inventario.InventarioNoFormalizado
                {
                    MaterialId = materialId,
                    Color = color,
                    StockActual = cantidadCompra,
                    CostoPromedio = costoCompra,
                    ValorInventario = cantidadCompra * costoCompra,
                    FechaCreacion = DateTime.Now,
                    FechaActualizacion = DateTime.Now
                };

                _context.InventariosNoFormalizados.Add(inventario);
                return;
            }

            var stockAnterior = inventario.StockActual;
            var costoAnterior = inventario.CostoPromedio;
            var nuevoStock = stockAnterior + cantidadCompra;

            var nuevoCostoPromedio =
                ((stockAnterior * costoAnterior) +
                 (cantidadCompra * costoCompra)) /
                nuevoStock;

            inventario.StockActual = nuevoStock;
            inventario.CostoPromedio = nuevoCostoPromedio;
            inventario.ValorInventario = nuevoStock * nuevoCostoPromedio;
            inventario.FechaActualizacion = DateTime.Now;
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
                    i.Material.Categoria.Contains(search) ||
                    i.Material.Densidad.Contains(search) ||
                    (i.Material.TipoProduccion != null &&
                     i.Material.TipoProduccion.Contains(search)) ||
                    i.Color.Contains(search));
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
                query = query.Where(i => i.Color == color);
            }

            var total = await query.CountAsync();

            decimal? totalKg = null;
            decimal? totalValorInventario = null;

            if (puedeVerDatosNumericos)
            {
                totalKg = await query.SumAsync(i => i.StockActual);
                totalValorInventario = await query.SumAsync(i => i.ValorInventario);
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
                    Material = i.Material!.NombreMaterial,
                    Proveedor = i.Material.ProveedorNoFormalizado!.Nombre,
                    Categoria = i.Material.Categoria,
                    Color = i.Color,
                    Densidad = i.Material.Densidad,
                    StockActual = puedeVerDatosNumericos
                        ? i.StockActual
                        : null,
                    CantidadComprometida = null,
                    StockDisponible = puedeVerDatosNumericos
                        ? i.StockActual
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
                    i.Material.Categoria.Contains(search) ||
                    i.Material.Densidad.Contains(search) ||
                    (i.Material.TipoProduccion != null &&
                     i.Material.TipoProduccion.Contains(search)) ||
                    i.Color.Contains(search));
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
                query = query.Where(i => i.Color == color);
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

            var ultimaColumna = puedeVerDatosNumericos ? "G" : "D";
            var cantidadColumnas = puedeVerDatosNumericos ? 7 : 4;

            ws.Range($"A1:{ultimaColumna}1").Merge();
            ws.Cell("A1").Value = empresa?.NombreEmpresa ?? "EMPRESA";
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
                    "Stock Actual",
                    "Costo Promedio",
                    "Valor Inventario"
                }
                : new[]
                {
                    "Material",
                    "Proveedor",
                    "Categoría",
                    "Color"
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

                if (puedeVerDatosNumericos)
                {
                    ws.Cell(fila, 5).Value = item.StockActual;
                    ws.Cell(fila, 6).Value = item.CostoPromedio;
                    ws.Cell(fila, 7).Value = item.ValorInventario;

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
                ws.Cell(fila + 1, 4).Value = "TOTAL";
                ws.Cell(fila + 1, 4).Style.Font.Bold = true;

                ws.Cell(fila + 1, 5).Value = totalStock;
                ws.Cell(fila + 1, 5).Style.Font.Bold = true;

                ws.Cell(fila + 1, 7).Value = totalValor;
                ws.Cell(fila + 1, 7).Style.Font.Bold = true;

                ws.Column(5).Style.NumberFormat.Format = "#,##0.00";
                ws.Column(6).Style.NumberFormat.Format = "$ #,##0";
                ws.Column(7).Style.NumberFormat.Format = "$ #,##0";
            }

            ws.Columns().AdjustToContents();
            ws.SheetView.FreezeRows(filaInicio);

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);

            return stream.ToArray();
        }
    }
}