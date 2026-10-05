using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SistemaFlota.Models.ComprasNoFormalizadas.RecepcionMercancias;
using SistemaFlota.Services.ComprasNoFormalizadas.ImpresionEtiquetasNoFormalizadas.Componentes;
using SistemaFlota.Services.ImpresionEtiquetas.Componentes;

namespace SistemaFlota.Services.EtiquetasQr.ComprasNoFormalizadas.Componentes
{
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
                    // 60% Información izquierda
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

                    // 40% QR derecha
                    row.RelativeItem(4)
                        .BorderRight(0.5f)
                        .AlignCenter()
                        .AlignMiddle()
                        .Element(qr =>
                        {
                            QrComponent.Dibujar(
                                qr,
                                $"RECEPCION:{recepcion.Id};DETALLE:{detalle.Id};BULTO:{numeroBulto}/{totalBultos}",
                                codigoFormateado);
                        });
                });
        }
    }
}
