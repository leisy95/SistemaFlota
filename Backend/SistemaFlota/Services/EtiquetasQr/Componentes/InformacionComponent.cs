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
            .AlignMiddle() // Volvemos a centrar verticalmente para equilibrar espacios
            .PaddingVertical(4) // Pequeño respiro arriba y abajo para que no pegue con los bordes del cuadro
            .Column(info =>
            {
                info.Spacing(2);

                // Proveedor
                info.Item()
                    .Text(recepcion.OrdenCompra!.Proveedor!.Nombre)
                    .Bold()
                    .FontSize(11)
                    .ClampLines(2);

                info.Item()
                    .PaddingTop(1)
                    .PaddingBottom(2)
                    .LineHorizontal(0.5f);

                // Material
                info.Item()
                    .Text($"Material: {detalle.OrdenCompraDetalle!.Material.NombreMaterial}")
                    .Bold()
                    .FontSize(9.5f)
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