using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SistemaFlota.Models.Costos.RecepcionMercancias;
using SistemaFlota.Services.ImpresionEtiquetas.Componentes;

namespace SistemaFlota.Services.Pdf.EtiquetasQR.Components;

public static class EtiquetaComponent
{
    public static void Dibujar(
        IContainer container,
        SistemaFlota.Models.Costos.RecepcionMercancias.RecepcionMercancia recepcion,
        RecepcionMercanciaDetalle detalle,
        int numeroBulto,
        int totalBultos,
        string codigoFormateado)
    {
        container
            .Border(1)
            .BorderColor(Colors.Grey.Darken2)
            .Padding(3)
            .Row(row =>
            {
                // 60% Información izquierda
                row.RelativeItem(6)
                    .PaddingRight(5)
                    .Element(info =>
                    {
                        InformacionComponent.Dibujar(
                            info,
                            recepcion,
                            detalle,
                            numeroBulto,
                            totalBultos);
                    });

                // 40% QR derecha
                row.RelativeItem(4)
                    .BorderRight(0.5f)
                    .AlignCenter()
                    .AlignMiddle()
                    .Element(qr =>
                    {
                        string numeroOrden =
                            recepcion.OrdenCompra!.Numero;

                        string fechaOrden =
                            recepcion.OrdenCompra.FechaOrden
                                .ToString("dd/MM/yyyy");

                        string lote =
                            detalle.LoteProveedor?.Trim() ?? "";

                        // Información completa que tendrá el QR
                        string contenidoQr =
                            $"Orden: {numeroOrden}\n" +
                            $"Fecha: {fechaOrden}\n" +
                            $"Lote: {lote}\n" +
                            $"Trazabilidad: {codigoFormateado}\n" +
                            $"Bulto: {numeroBulto}/{totalBultos}";

                        QrComponent.Dibujar(
                            qr,
                            contenidoQr,
                            codigoFormateado);
                    });
            });
    }
}