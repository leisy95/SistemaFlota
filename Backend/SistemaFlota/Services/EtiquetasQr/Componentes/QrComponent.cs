using QRCoder;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace SistemaFlota.Services.ImpresionEtiquetas.Componentes;

public static class QrComponent
{
    public static void Dibujar(
        IContainer container,
        string contenidoQr,
        string codigo)
    {
        using var qrGenerator = new QRCodeGenerator();

        using var qrData = qrGenerator.CreateQrCode(
            contenidoQr,
            QRCodeGenerator.ECCLevel.Q);

        using var qrCode = new PngByteQRCode(qrData);

        byte[] qrBytes = qrCode.GetGraphic(10);

        container
            .AlignCenter()
            .Column(column =>
            {
                column.Item()
                    .Width(35, Unit.Millimetre)
                    .Height(35, Unit.Millimetre)
                    .Image(qrBytes);

                column.Item()
                    .PaddingTop(1)
                    .AlignCenter()
                    .Text("CÓDIGO DE TRAZABILIDAD")
                    .FontSize(5.5f)
                    .FontColor(Colors.Grey.Darken2);

                column.Item()
                    .PaddingTop(0.5f)
                    .AlignCenter()
                    .Text(codigo)
                    .FontSize(8)
                    .Bold();
            });
    }
}