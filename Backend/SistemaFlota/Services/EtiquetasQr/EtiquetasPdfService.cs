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
        var recepcion = await _context.RecepcionesMercancia
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
            int numeroEtiqueta = 1;

            foreach (var detalle in recepcion.Detalles)
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
                                numeroEtiqueta);
                        });
                });

                numeroEtiqueta++;
            }
        });

        return document.GeneratePdf();
    }
}