using QRCoder;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace SistemaFlota.Services.ImpresionEtiquetas.Componentes;

public static class QrComponent
{
    public static void Dibujar(
        IContainer container,
        string contenidoQr)
    {
        using var qrGenerator = new QRCodeGenerator();

        using var qrData = qrGenerator.CreateQrCode(
            contenidoQr,
            QRCodeGenerator.ECCLevel.Q);

        using var qrCode = new PngByteQRCode(qrData);

        byte[] qrBytes = qrCode.GetGraphic(10);

        container
            .Padding(3)
            .AlignCenter()
            .AlignMiddle()
            .Width(35, Unit.Millimetre)
            .Height(35, Unit.Millimetre)
            .Image(qrBytes);
    }
}