
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SistemaFlota.Models.Costos.RecepcionMercancias;

namespace SistemaFlota.Services.Pdf.EtiquetasQR.Components;

public static class InformacionComponent
{
    public static void Dibujar(
        IContainer container,
        SistemaFlota.Models.Costos.RecepcionMercancias.RecepcionMercancia recepcion,
        RecepcionMercanciaDetalle detalle,
        int numeroBulto,
        int totalBultos)
    {
        container
            .AlignMiddle()
            .Column(info =>
            {
                info.Spacing(1);

                // Proveedor
                info.Item()
                    .Text(recepcion.OrdenCompra!.Proveedor!.Nombre)
                    .Bold()
                    .FontSize(12)
                    .ClampLines(2);

                info.Item()
                    .LineHorizontal(0.5f);

                // Material
                info.Item()
                    .Text($"Material: {detalle.OrdenCompraDetalle!.Material.NombreMaterial}")
                    .Bold()
                    .FontSize(10)
                    .ClampLines(2);

                // Color
                info.Item()
                    .Text($"Color: {detalle.OrdenCompraDetalle.Material.Color}")
                    .FontSize(8)
                    .ClampLines(1);

                // Fecha
                info.Item()
                    .Text($"Fecha: {recepcion.FechaRecepcion:dd/MM/yyyy}")
                    .FontSize(8);

                // Bulto
                info.Item()
                    .Text($"Bulto: {numeroBulto}/{totalBultos}")
                    .Bold()
                    .FontSize(8);

                // Peso
                var pesoPorBulto = totalBultos > 0
                    ? detalle.CantidadRecibida / totalBultos
                    : 0;

                info.Item()
                    .Text($"Peso: {pesoPorBulto:0.##} Kg")
                    .Bold()
                    .FontSize(8);
            });
    }
}
