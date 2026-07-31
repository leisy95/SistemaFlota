using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SistemaFlota.Services.Pdf.Styles;

namespace SistemaFlota.Services.Pdf.Components;

public static class EstadoBadge
{
    public static void Dibujar(
        IContainer container,
        string estado)
    {
        var color = estado switch
        {
            "Pendiente" => PdfColors.Amarillo,
            "Aprobada" => PdfColors.VerdePrincipal,
            "Anulada" => PdfColors.Rojo,
            _ => PdfColors.GrisMedio
        };

        container
           .MinWidth(90)
           .Background("#FEF3C7")
           .Border(1)
           .BorderColor("#F59E0B")
           .CornerRadius(10)
           .PaddingVertical(5)
           .PaddingHorizontal(15)
           .AlignCenter()
           .Text(estado.ToUpper())
           .FontSize(9)
           .Bold()
           .FontColor("#92400E");
    }
}