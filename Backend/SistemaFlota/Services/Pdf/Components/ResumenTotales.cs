using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using SistemaFlota.Services.Pdf.Styles;
using System.Globalization;

namespace SistemaFlota.Services.Pdf.Components;

public static class ResumenTotales
{
    private static readonly CultureInfo CulturaColombia = new("es-CO");

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
            Fila(col, "Subtotal", FormatearPesos(subtotal));

            Fila(
                col,
                $"{tipoImpuesto} ({porcentajeImpuesto:0.##}%)",
                FormatearPesos(valorImpuesto)
            );

            col.Item()
                .PaddingVertical(5)
                .LineHorizontal(1)
                .LineColor(PdfColors.GrisClaro);

            Fila(col, "TOTAL", FormatearPesos(total), true);
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

    private static string FormatearPesos(decimal valor)
    {
        return valor.ToString("C0", CulturaColombia);
    }
}
