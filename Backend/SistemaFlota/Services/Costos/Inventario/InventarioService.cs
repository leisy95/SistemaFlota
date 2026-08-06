using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using SistemaFlota.DTOs.Costos.Inventario;
using SistemaFlota.Models.Costos.RecepcionMercancias;
using SistemaFlota.Services.Auth;

namespace SistemaFlota.Services.Costos.Inventario
{
    public class InventarioService : IInventarioService
    {
        private readonly AppDbContext _context;
        private readonly ICurrentUserService _currentUser;

        public InventarioService(AppDbContext context,
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

        private async Task<Models.Costos.RecepcionMercancias.RecepcionMercancia> ObtenerRecepcionAsync(int recepcionId)
        {
            var recepcion = await _context.RecepcionesMercancias
                .Include(r => r.Detalles)
                    .ThenInclude(d => d.OrdenCompraDetalle)
                        .ThenInclude(o => o.Material)
                .FirstOrDefaultAsync(r => r.Id == recepcionId);

            if (recepcion == null)
                throw new Exception("La recepción no existe.");

            return recepcion;
        }

        private async Task ActualizarInventarioAsync(RecepcionMercanciaDetalle detalle)
        {
            var materialId = detalle.OrdenCompraDetalle!.MaterialId;
            var color = detalle.OrdenCompraDetalle.Color;
            var cantidadCompra = detalle.CantidadRecibida;
            var costoCompra = detalle.OrdenCompraDetalle.CostoKg;

            var inventario = await _context.Inventarios
                .FirstOrDefaultAsync(
                i => i.MaterialId == materialId &&
                i.Color == color);

            // Primera entrada del material
            if (inventario == null)
            {
                inventario = new Models.Costos.Inventario.Inventario
                {
                    MaterialId = materialId,
                    Color = color,
                    StockActual = cantidadCompra,
                    CostoPromedio = costoCompra,
                    ValorInventario = cantidadCompra * costoCompra,
                    FechaCreacion = DateTime.Now,
                    FechaActualizacion = DateTime.Now
                };

                _context.Inventarios.Add(inventario);
                return;
            }

            var stockAnterior = inventario.StockActual;
            var costoAnterior = inventario.CostoPromedio;

            var nuevoStock = stockAnterior + cantidadCompra;

            var nuevoCostoPromedio =
                ((stockAnterior * costoAnterior) +
                 (cantidadCompra * costoCompra))
                / nuevoStock;

            inventario.StockActual = nuevoStock;
            inventario.CostoPromedio = nuevoCostoPromedio;
            inventario.ValorInventario = nuevoStock * nuevoCostoPromedio;
            inventario.FechaActualizacion = DateTime.Now;
        }

        public async Task<InventarioPaginadoDto> ObtenerAsync(
             string? search,
             int? proveedorId,
             string? categoria,
             string? color,
             int page,
             int pageSize)
        {
            var query = _context.Inventarios
                .AsNoTracking()
                .Include(i => i.Material)
                    .ThenInclude(m => m.Proveedor)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();

                query = query.Where(i =>
                    i.Material!.NombreMaterial.Contains(search) ||
                    (i.Material.DescripcionCompra != null &&
                     i.Material.DescripcionCompra.Contains(search)) ||
                    i.Material.Proveedor!.Nombre.Contains(search) ||
                    i.Color.Contains(search) ||
                    i.Material.Categoria.Contains(search));
            }

            if (proveedorId.HasValue)
                query = query.Where(i => i.Material!.IdProveedor == proveedorId);

            if (!string.IsNullOrWhiteSpace(categoria))
                query = query.Where(i => i.Material!.Categoria == categoria);

            if (!string.IsNullOrWhiteSpace(color))
                query = query.Where(i => i.Color == color);

            var total = await query.CountAsync();

            var items = await query
                .OrderBy(i => i.Material!.NombreMaterial)
                .ThenBy(i => i.Color)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(i => new InventarioDto
                {
                    Id = i.Id,
                    MaterialId = i.MaterialId,
                    Material = i.Material!.DescripcionCompra ?? "",
                    Proveedor = i.Material.Proveedor!.Nombre,
                    Tipo = i.Material.Categoria,
                    Color = i.Color,
                    Densidad = i.Material.Densidad,
                    StockActual = i.StockActual,
                    CostoPromedio = i.CostoPromedio,
                    ValorInventario = i.ValorInventario
                })
                .ToListAsync();

            return new InventarioPaginadoDto
            {
                Items = items,
                Total = total,
                Pagina = page,
                PageSize = pageSize
            }; 
        }

        public async Task<List<ProveedorFiltroDto>> ObtenerProveedoresInventarioAsync()
        {
            return await _context.Inventarios
                .AsNoTracking()
                .Include(i => i.Material)
                    .ThenInclude(m => m.Proveedor)
                .Select(i => new ProveedorFiltroDto
                {
                    Id = i.Material!.IdProveedor,
                    Nombre = i.Material.Proveedor!.Nombre
                })
                .Distinct()
                .OrderBy(p => p.Nombre)
                .ToListAsync();
        }

        public async Task<List<string>> ObtenerCategoriasInventarioAsync()
        {
            return await _context.Inventarios
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
             string? color)
        {
            var query = _context.Inventarios
                .AsNoTracking()
                .Include(i => i.Material)
                    .ThenInclude(m => m.Proveedor)
                .AsQueryable();


            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();

                query = query.Where(i =>
                    i.Material!.NombreMaterial.Contains(search) ||
                    (i.Material.DescripcionCompra != null &&
                     i.Material.DescripcionCompra.Contains(search)) ||
                    i.Material.Proveedor!.Nombre.Contains(search) ||
                    i.Color.Contains(search) ||
                    i.Material.Categoria.Contains(search));
            }

            if (proveedorId.HasValue)
                query = query.Where(i => i.Material!.IdProveedor == proveedorId);


            if (!string.IsNullOrWhiteSpace(categoria))
                query = query.Where(i => i.Material!.Categoria == categoria);


            if (!string.IsNullOrWhiteSpace(color))
                query = query.Where(i => i.Color == color);

            var inventario = await query
                .OrderBy(i => i.Material!.NombreMaterial)
                .ThenBy(i => i.Color)
                .ToListAsync();

            var empresa = await _context.ConfiguracionEmpresa
                .FirstOrDefaultAsync();

            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(
                    u => u.Id == _currentUser.IdUsuario
                );

            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("Reporte Inventario");

            // ENCABEZADO EMPRESA
            ws.Range("A1:H1").Merge();

            ws.Cell("A1").Value =
                empresa?.NombreEmpresa ?? "EMPRESA";

            ws.Cell("A1").Style.Font.Bold = true;
            ws.Cell("A1").Style.Font.FontSize = 18;
            ws.Cell("A1").Style.Alignment.Horizontal =
                XLAlignmentHorizontalValues.Center;

            ws.Range("A2:H2").Merge();

            ws.Cell("A2").Value =
                "REPORTE DE INVENTARIO";

            ws.Cell("A2").Style.Font.Bold = true;
            ws.Cell("A2").Style.Font.FontSize = 14;
            ws.Cell("A2").Style.Alignment.Horizontal =
                XLAlignmentHorizontalValues.Center;



            ws.Range("A3:H3").Merge();

            ws.Cell("A3").Value =
                $"Fecha exportación: {DateTime.Now:dd/MM/yyyy HH:mm}";

            ws.Range("A4:H4").Merge();

            ws.Cell("A4").Value =
                $"Usuario exportación: {usuario?.Username ?? "Sistema"}";

            ws.Range("A5:H5").Merge();

            ws.Cell("A5").Value =
                $"Filtros aplicados: " +
                $"Búsqueda={search ?? "Todos"} | " +
                $"Categoría={categoria ?? "Todas"} | " +
                $"Color={color ?? "Todos"}";

            var filaInicio = 7;

            // TABLA
            var headers = new[]
            {
                "Material",
                "Proveedor",
                "Categoría",
                "Color",
                "Densidad",
                "Stock Actual",
                "Costo Promedio",
                "Valor Inventario"
            };

            for (int i = 0; i < headers.Length; i++)
            {
                ws.Cell(filaInicio, i + 1)
                    .Value = headers[i];
            }

            var header = ws.Range(
                filaInicio,
                1,
                filaInicio,
                8);

            header.Style.Fill.BackgroundColor =
                XLColor.FromHtml("#1F4E78");


            header.Style.Font.FontColor =
                XLColor.White;

            header.Style.Font.Bold = true;

            header.Style.Alignment.Horizontal =
                XLAlignmentHorizontalValues.Center;

            int fila = filaInicio + 1;

            decimal totalStock = 0;
            decimal totalValor = 0;

            foreach (var item in inventario)
            {
                ws.Cell(fila, 1)
                    .Value = item.Material?.DescripcionCompra;

                ws.Cell(fila, 2)
                    .Value = item.Material?.Proveedor?.Nombre;

                ws.Cell(fila, 3)
                    .Value = item.Material?.Categoria;


                ws.Cell(fila, 4)
                    .Value = item.Color;

                ws.Cell(fila, 5)
                    .Value = item.Material?.Densidad;

                ws.Cell(fila, 6)
                    .Value = item.StockActual;

                ws.Cell(fila, 7)
                    .Value = item.CostoPromedio;

                ws.Cell(fila, 8)
                    .Value = item.ValorInventario;

                totalStock += item.StockActual;
                totalValor += item.ValorInventario;

                fila++;
            }

            // TABLA CON FILTROS

            var tabla = ws.Range(
                filaInicio,
                1,
                fila - 1,
                8);
            tabla.CreateTable();


            ws.Cell(fila + 1, 5)
                .Value = "TOTAL";


            ws.Cell(fila + 1, 5)
                .Style.Font.Bold = true;

            ws.Cell(fila + 1, 6)
                .Value = totalStock;
            ws.Cell(fila + 1, 8)
                .Value = totalValor;
            ws.Cell(fila + 1, 6)
                .Style.Font.Bold = true;
            ws.Cell(fila + 1, 8)
                .Style.Font.Bold = true;

            ws.Column(6)
                .Style.NumberFormat.Format =
                "#,##0.00";

            ws.Column(7)
                .Style.NumberFormat.Format =
                "$ #,##0";

            ws.Column(8)
                .Style.NumberFormat.Format =
                "$ #,##0";

            ws.Columns()
                .AdjustToContents();

            ws.SheetView.FreezeRows(filaInicio);

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);

            return stream.ToArray();
        }
    }
}