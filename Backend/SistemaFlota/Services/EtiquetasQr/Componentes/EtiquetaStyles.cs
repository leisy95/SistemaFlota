using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace SistemaFlota.Services.Pdf.EtiquetasQR.Components;

public static class EtiquetaStyles
{
    // Empresa
    public static TextStyle Empresa =>
        TextStyle.Default
            .FontSize(14)
            .Bold()
            .FontColor(Colors.Blue.Darken3);

    // Título de cada sección
    public static TextStyle Label =>
        TextStyle.Default
            .FontSize(7)
            .SemiBold()
            .FontColor(Colors.Grey.Darken2);

    // Valor de cada campo
    public static TextStyle Valor =>
        TextStyle.Default
            .FontSize(8)
            .FontColor(Colors.Black);

    // Nombre del material
    public static TextStyle Material =>
        TextStyle.Default
            .FontSize(10)
            .Bold();

    // Información pequeña
    public static TextStyle Pequeño =>
        TextStyle.Default
            .FontSize(7);

    // Pie de la etiqueta
    public static TextStyle Pie =>
    new TextStyle()
        .FontSize(9)
        .Bold();

    // Número de bulto
    public static TextStyle Bulto =>
        TextStyle.Default
            .FontSize(11)
            .Bold()
            .FontColor(Colors.Blue.Darken3);
}