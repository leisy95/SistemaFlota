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

        Console.WriteLine($"Recepción: {recepcion.Id}");
        Console.WriteLine($"Detalles encontrados: {recepcion.Detalles.Count}");

        var document = Document.Create(document =>
        {
            foreach (var detalle in recepcion.Detalles)
            {
                // Cantidad de bultos recibidos para este ítem
                int totalBultos = (int)detalle.BultosRecibidos;

                // Una etiqueta por cada bulto
                for (int numeroBulto = 1; numeroBulto <= totalBultos; numeroBulto++)
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
                                    totalBultos);
                            });
                    });
                }
            }
        });

        return document.GeneratePdf();
    }
}