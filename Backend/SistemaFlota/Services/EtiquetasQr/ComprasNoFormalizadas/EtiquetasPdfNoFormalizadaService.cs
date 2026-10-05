using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SistemaFlota.Services.ComprasNoFormalizadas.ImpresionEtiquetasNoFormalizadas.Componentes;
using SistemaFlota.Services.Pdf.EtiquetasQR.Components;

namespace SistemaFlota.Services.EtiquetasQr.ComprasNoFormalizadas.EtiquetasNoFormalizadas;

public class EtiquetasPdfNoFormalizadaService : IEtiquetasPdfNoFormalizadaService
{
    private readonly AppDbContext _context;

    public EtiquetasPdfNoFormalizadaService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<byte[]> GenerarAsync(int recepcionId)
    {
        var recepcion = await _context.RecepcionesMercanciasNoFormalizadas
            .Include(x => x.Detalles)
                .ThenInclude(x => x.OrdenCompraDetalleNoFormalizada)
                    .ThenInclude(x => x.MaterialNoFormalizado)
            .Include(x => x.OrdenCompraNoFormalizada)
                .ThenInclude(x => x.ProveedorNoFormalizado)
            .FirstOrDefaultAsync(x => x.Id == recepcionId);

        if (recepcion == null)
            throw new Exception("Recepción no formalizada no encontrada.");

        var document = Document.Create(document =>
        {
            foreach (var detalle in recepcion.Detalles)
            {
                int totalBultos = (int)detalle.BultosRecibidos;

                string numeroOrden =
                    recepcion.OrdenCompraNoFormalizada!.Numero;

                string ultimosTresOrden = numeroOrden.Length >= 3
                    ? numeroOrden[^3..]
                    : numeroOrden.PadLeft(3, '0');

                string diaMes = recepcion
                    .OrdenCompraNoFormalizada
                    .FechaOrden
                    .ToString("ddMM");

                string lote = detalle.LoteProveedor?.Trim() ?? "";

                string ultimoNumeroLote = "";

                if (!string.IsNullOrWhiteSpace(lote))
                {
                    ultimoNumeroLote = lote
                        .Split('-')
                        .Last()
                        .Trim();
                }

                string codigo =
                    $"{ultimosTresOrden}{diaMes}{ultimoNumeroLote}";

                string codigoFormateado =
                    $"{ultimosTresOrden} · " +
                    $"{diaMes[..2]} · " +
                    $"{diaMes[2..]} · " +
                    $"{ultimoNumeroLote}";

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
                                EtiquetaNoFormalizadaComponent.Dibujar(
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