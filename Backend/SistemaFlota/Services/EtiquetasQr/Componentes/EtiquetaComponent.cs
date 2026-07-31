using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SistemaFlota.Models.Costos.RecepcionMercancia;

namespace SistemaFlota.Services.Pdf.EtiquetasQR.Components;

public static class EtiquetaComponent
{
    public static void Dibujar(
    IContainer container,
    RecepcionMercancia recepcion,
    RecepcionMercanciaDetalle detalle,
    int numeroEtiqueta)
    {
        container
            .Border(1)
            .BorderColor(Colors.Grey.Darken2)
            .Padding(4)
            .Row(row =>
            {
                // LADO IZQUIERDO
                row.RelativeItem()
                    .Column(col =>
                    {
                        col.Item()
                            .Text("PLASTIERP")
                            .Bold()
                            .FontSize(9);

                        col.Item()
                            .PaddingVertical(2)
                            .LineHorizontal(0.5f);

                        col.Item()
                            .Text(detalle.OrdenCompraDetalle!
                                .Material.NombreMaterial)
                            .Bold()
                            .FontSize(10);

                        col.Item()
                            .PaddingTop(3)
                            .Text($"Orden: {recepcion.OrdenCompra!.Numero}")
                            .FontSize(7);

                        col.Item()
                            .Text($"Recepción: {recepcion.NumeroRecepcion}")
                            .FontSize(7);

                        col.Item()
                            .Text($"Lote: {detalle.LoteProveedor}")
                            .FontSize(7);

                        col.Item()
                            .Text($"Cantidad: {detalle.CantidadRecibida:0.##} Kg")
                            .FontSize(7);

                        col.Item()
                            .Text($"Etiqueta: {numeroEtiqueta}")
                            .FontSize(7);
                    });


                // QR
                row.ConstantItem(65)
                    .AlignCenter()
                    .AlignMiddle()
                    .BorderLeft(0.5f)
                    .PaddingLeft(5)
                    .Text("QR")
                    .FontSize(8);
            });
    }
}