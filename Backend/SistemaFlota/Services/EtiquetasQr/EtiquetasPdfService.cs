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
        var recepcion = await _context.RecepcionesMercancias
            .Include(x => x.Detalles)
                .ThenInclude(x => x.OrdenCompraDetalle)
                    .ThenInclude(x => x.Material)
            .Include(x => x.OrdenCompra)
                .ThenInclude(x => x.Proveedor)
            .FirstAsync(x => x.Id == recepcionId);

        var document = Document.Create(document =>
        {
            foreach (var detalle in recepcion.Detalles)
            {
                int totalBultos = (int)detalle.BultosRecibidos;

                // ÚLTIMOS 3 CARACTERES DE LA ORDEN

                string numeroOrden = recepcion.OrdenCompra!.Numero;

                string ultimosTresOrden = numeroOrden.Length >= 3
                    ? numeroOrden[^3..]
                    : numeroOrden.PadLeft(3, '0');


                // DÍA Y MES DE LA FECHA DE LA ORDEN

                string diaMes = recepcion.OrdenCompra.FechaOrden
                    .ToString("ddMM");


                // ÚLTIMO NÚMERO DEL LOTE

                string lote = detalle.LoteProveedor?.Trim() ?? "";

                string ultimoNumeroLote = "";

                if (!string.IsNullOrWhiteSpace(lote))
                {
                    ultimoNumeroLote = lote
                        .Split('-')
                        .Last()
                        .Trim();
                }


                // CÓDIGO FINAL

                string codigo =
                    $"{ultimosTresOrden}{diaMes}{ultimoNumeroLote}";

                string codigoFormateado =
                    $"{ultimosTresOrden} · {diaMes[..2]} · {diaMes[2..]} · {ultimoNumeroLote}";


                // UNA ETIQUETA POR CADA BULTO

                for (
                    int numeroBulto = 1;
                    numeroBulto <= totalBultos;
                    numeroBulto++)
                {
                    document.Page(page =>
                    {
                        page.Size(100, 50, Unit.Millimetre);

                        page.Margin(2, Unit.Millimetre);

                        page.Content()
                            .Element(container =>
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