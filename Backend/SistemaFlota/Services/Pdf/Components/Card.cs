using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SistemaFlota.Services.Pdf.Styles;

namespace SistemaFlota.Services.Pdf.Components;

public static class Card
{
    public static void Dibujar(
        IContainer container,
        string titulo,
        Action<ColumnDescriptor> contenido)
    {
        container
            .Border(1)
            .BorderColor(PdfColors.GrisClaro)
            .Column(col =>
            {
                // Encabezado
                col.Item()
                    .Background(PdfColors.AzulOscuro)
                    .PaddingVertical(6)
                    .PaddingHorizontal(10)
                    .Text(titulo)
                    .Style(PdfStyles.HeaderTabla);

                // Contenido
                col.Item()
                    .Padding(10)
                    .Column(contenido);
            });
    }
}