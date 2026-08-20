using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SistemaFlota.Services.Pdf.Styles;

namespace SistemaFlota.Services.Pdf.Components;

public static class ResumenTotales
{
    public static void Dibujar(
        IContainer container,
        decimal subtotal,
        string tipoImpuesto,
        decimal porcentajeImpuesto,
        decimal valorImpuesto,
        decimal total)
    {
        container.AlignRight().Width(250).Column(col =>
        {
            Fila(col, "Subtotal", subtotal.ToString("C0"));

            Fila(
                col,
                $"{tipoImpuesto} ({porcentajeImpuesto:0.##}%)",
                valorImpuesto.ToString("C0")
            );

            col.Item()
                .PaddingVertical(5)
                .LineHorizontal(1)
                .LineColor(PdfColors.GrisClaro);

            Fila(col, "TOTAL", total.ToString("C0"), true);
        });
    }

    private static void Fila(
        ColumnDescriptor col,
        string titulo,
        string valor,
        bool total = false)
    {
        col.Item().Row(row =>
        {
            row.RelativeItem()
                .Text(titulo)
                .Style(total ? PdfStyles.Total : PdfStyles.Label);

            row.ConstantItem(100)
                .AlignRight()
                .Text(valor)
                .Style(total ? PdfStyles.Total : PdfStyles.Valor);
        });
    }
}