using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SistemaFlota;
using SistemaFlota.Services.Pdf.EtiquetasQR.Components;

namespace SistemaFlota.Services.ImpresionEtiquetas;

public class EtiquetasPdfService : IEtiquetasPdfService
{
    private readonly AppDbContext _context;

    public EtiquetasPdfService(AppDbContext context)
    {
        _context = context;
    }


    public async Task<byte[]> GenerarAsync(int recepcionId)
    {
        // 1. Cargar recepción con sus detalles
        var recepcion = await _context.RecepcionesMercancias
            .AsNoTracking()
            .Include(x => x.Detalles)
                .ThenInclude(x => x.OrdenCompraDetalle)
                    .ThenInclude(x => x.Material)
            .Include(x => x.OrdenCompra)
                .ThenInclude(x => x.Proveedor)
            .FirstAsync(x => x.Id == recepcionId);

        // 2. Identificar la última entrega
        int ultimaEntregaActual = recepcion.Detalles.Any()
            ? recepcion.Detalles.Max(x => x.NumeroEntrega)
            : 1;

        // 3. Obtener lotes de la última entrega
        var detallesEntregaActual = recepcion.Detalles
            .Where(x => x.NumeroEntrega == ultimaEntregaActual)
            .OrderBy(x => x.Id)
            .ToList();

        // 4. Obtener bultos de entregas anteriores
        var detallesAnteriores = await _context.RecepcionesMercancias
            .AsNoTracking()
            .Where(x => x.OrdenCompraId == recepcion.OrdenCompraId)
            .SelectMany(x => x.Detalles)
            .Where(x => x.NumeroEntrega < ultimaEntregaActual)
            .ToListAsync();

        var document = Document.Create(document =>
        {
            foreach (var detalle in detallesEntregaActual)
            {
                int bultosActuales = (int)detalle.BultosRecibidos;
                int ordenCompraDetalleId = detalle.OrdenCompraDetalleId;

                // 5. Sumar bultos de entregas anteriores
                int bultosAnteriores = detallesAnteriores
                    .Where(x => x.OrdenCompraDetalleId == ordenCompraDetalleId)
                    .Sum(x => (int)x.BultosRecibidos);

                // 6. Sumar bultos de lotes anteriores de esta entrega
                bultosAnteriores += detallesEntregaActual
                    .Where(x =>
                        x.OrdenCompraDetalleId == ordenCompraDetalleId &&
                        x.Id < detalle.Id)
                    .Sum(x => (int)x.BultosRecibidos);

                // 7. Total de bultos de la orden
                int totalBultos = (int)detalle.OrdenCompraDetalle!.Bultos;

                // 8. Código de trazabilidad
                string numeroOrden = recepcion.OrdenCompra!.Numero;

                string ultimosTresOrden = numeroOrden.Length >= 3
                    ? numeroOrden[^3..]
                    : numeroOrden.PadLeft(3, '0');

                string dia = recepcion.FechaRecepcion.ToString("MM");
                string año = recepcion.FechaRecepcion.ToString("yy");

                string lote = detalle.LoteProveedor?.Trim() ?? "";

                string ultimoNumeroLote = "";

                if (!string.IsNullOrWhiteSpace(lote))
                {
                    ultimoNumeroLote = lote
                        .Split('-')
                        .Last()
                        .Trim();
                }

                string codigoFormateado =
                    $"{ultimosTresOrden} · {dia} · {año} · {ultimoNumeroLote}";

                // 9. Generar una página por cada bulto
                for (int i = 1; i <= bultosActuales; i++)
                {
                    int numeroBulto = bultosAnteriores + i;

                    document.Page(page =>
                    {
                        page.Size(100, 50, Unit.Millimetre);
                        page.Margin(2, Unit.Millimetre);

                        page.Content().Element(container =>
                        {
                            EtiquetaComponent.Dibujar(
                                container,
                                recepcion,
                                detalle,
                                numeroBulto,
                                totalBultos,
                                codigoFormateado);
                        });
                    });
                }
            }
        });

        return document.GeneratePdf();
    }


    // Para generar todas las etiquetas cuando ya esten confirmadas
    public async Task<byte[]> GenerarTodasAsync(int recepcionId)
    {
        var recepcion = await _context.RecepcionesMercancias
            .AsNoTracking()
            .Include(x => x.Detalles)
                .ThenInclude(x => x.OrdenCompraDetalle)
                    .ThenInclude(x => x.Material)
            .Include(x => x.OrdenCompra)
                .ThenInclude(x => x.Proveedor)
            .FirstOrDefaultAsync(x => x.Id == recepcionId);

        if (recepcion == null)
            throw new InvalidOperationException(
                "La recepción solicitada no existe.");

        if (recepcion.OrdenCompra == null)
            throw new InvalidOperationException(
                "La recepción no tiene una orden de compra asociada.");

        // Obtener todos los detalles de todas las entregas.
        var todosLosDetalles = recepcion.Detalles
            .OrderBy(x => x.NumeroEntrega)
            .ThenBy(x => x.Id)
            .ToList();

        if (!todosLosDetalles.Any())
            throw new InvalidOperationException(
                "La recepción no tiene detalles para generar etiquetas.");

        // Crear un PDF con todas las etiquetas.
        var document = Document.Create(document =>
        {
            foreach (var detalle in todosLosDetalles)
            {
                int bultosActuales = (int)detalle.BultosRecibidos;

                int ordenCompraDetalleId = detalle.OrdenCompraDetalleId;

                // Sumar los bultos de entregas anteriores
                // del mismo material/detalle de la orden.
                int bultosAnteriores = todosLosDetalles
                    .Where(x =>
                        x.OrdenCompraDetalleId == ordenCompraDetalleId &&
                        x.NumeroEntrega < detalle.NumeroEntrega)
                    .Sum(x => (int)x.BultosRecibidos);

                // Total de bultos programados en la orden.
                int totalBultos = (int)detalle.OrdenCompraDetalle!.Bultos;

                string numeroOrden = recepcion.OrdenCompra.Numero;

                string ultimosTresOrden = numeroOrden.Length >= 3
                    ? numeroOrden[^3..]
                    : numeroOrden.PadLeft(3, '0');

                string dia = recepcion.OrdenCompra.FechaOrden.ToString("dd");
                string año = recepcion.OrdenCompra.FechaOrden.ToString("yy");

                string lote = detalle.LoteProveedor?.Trim() ?? "";
                string ultimoNumeroLote = "";

                if (!string.IsNullOrWhiteSpace(lote))
                {
                    ultimoNumeroLote = lote
                        .Split('-')
                        .Last()
                        .Trim();
                }

                string codigoFormateado =
                    $"{ultimosTresOrden} · {dia} · {año} · {ultimoNumeroLote}";

                // Crear una página por cada bulto.
                for (int i = 1; i <= bultosActuales; i++)
                {
                    int numeroBulto = bultosAnteriores + i;

                    document.Page(page =>
                    {
                        page.Size(100, 50, Unit.Millimetre);
                        page.Margin(2, Unit.Millimetre);

                        page.Content().Element(container =>
                        {
                            EtiquetaComponent.Dibujar(
                                container,
                                recepcion,
                                detalle,
                                numeroBulto,
                                totalBultos,
                                codigoFormateado);
                        });
                    });
                }
            }
        });

        return document.GeneratePdf();
    }

}