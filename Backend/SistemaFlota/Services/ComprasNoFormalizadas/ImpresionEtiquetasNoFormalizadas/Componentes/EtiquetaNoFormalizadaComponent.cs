using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SistemaFlota.Models.ComprasNoFormalizadas.RecepcionMercancias;
using SistemaFlota.Services.ImpresionEtiquetas.Componentes;

namespace SistemaFlota.Services.ComprasNoFormalizadas.ImpresionEtiquetasNoFormalizadas.Componentes;

public static class EtiquetaNoFormalizadaComponent
{
    public static void Dibujar(
        IContainer container,
        RecepcionMercanciaNoFormalizada recepcion,
        RecepcionMercanciaDetalleNoFormalizada detalle,
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
                row.RelativeItem(6)
                    .PaddingRight(5)
                    .Element(info =>
                    {
                        InformacionNoFormalizadaComponent.Dibujar(
                            info,
                            recepcion,
                            detalle,
                            numeroBulto,
                            totalBultos);
                    });

                row.RelativeItem(4)
                    .BorderRight(0.5f)
                    .AlignCenter()
                    .AlignMiddle()
                    .Element(qr =>
                    {
                        string numeroOrden =
                            recepcion
                                .OrdenCompraNoFormalizada!
                                .Numero;

                        string fechaOrden =
                            recepcion
                                .OrdenCompraNoFormalizada
                                .FechaOrden
                                .ToString("dd/MM/yyyy");

                        string lote =
                            detalle.LoteProveedor?.Trim() ?? "";

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