using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace SistemaFlota.Services.Pdf.Styles
{
    public class PdfStyles
    {
        // Título principal
        public static TextStyle Titulo =>
            TextStyle.Default
                .FontSize(18)
                .SemiBold()
                .FontColor(PdfColors.AzulOscuro);

        // Título de sección
        public static TextStyle Subtitulo =>
            TextStyle.Default
                .FontSize(12)
                .SemiBold()
                .FontColor(PdfColors.VerdeOscuro);

        // Etiquetas
        public static TextStyle Label =>
            TextStyle.Default
                .FontSize(9)
                .FontColor(PdfColors.GrisOscuro);

        // Valores
        public static TextStyle Valor =>
            TextStyle.Default
                .FontSize(10)
                .FontColor(PdfColors.AzulOscuro);

        // Encabezado de tabla
        public static TextStyle HeaderTabla =>
            TextStyle.Default
                .FontSize(10)
                .SemiBold()
                .FontColor(PdfColors.Blanco);

        // Filas de tabla
        public static TextStyle CeldaTabla =>
            TextStyle.Default
                .FontSize(9);

        // Total
        public static TextStyle Total =>
            TextStyle.Default
                .FontSize(13)
                .SemiBold()
                .FontColor(PdfColors.VerdeOscuro);

        // Footer
        public static TextStyle Footer =>
            TextStyle.Default
                .FontSize(8)
                .FontColor(PdfColors.GrisMedio);
    }
}
