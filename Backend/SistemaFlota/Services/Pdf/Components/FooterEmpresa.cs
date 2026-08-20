using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SistemaFlota.Services.Pdf.Styles;

namespace SistemaFlota.Services.Pdf.Components;

public static class FooterEmpresa
{
    public static void Dibujar(IContainer container)
    {
        container
            .BorderTop(1)
            .BorderColor(PdfColors.GrisClaro)
            .PaddingTop(8)
            .Row(row =>
            {
                row.RelativeItem()
                    .Text("Sistema Flota - Orden de Compra")
                    .Style(PdfStyles.Footer);

                row.ConstantItem(120)
                    .AlignRight()
                    .Text(text =>
                    {
                        text.DefaultTextStyle(PdfStyles.Footer);

                        text.Span("Página ");
                        text.CurrentPageNumber();
                        text.Span(" de ");
                        text.TotalPages();
                    });
            });
    }
}