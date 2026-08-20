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
    int numeroEtiqueta)
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
                    .FontSize(14);

                info.Item()
                    .LineHorizontal(0.5f);

                // Material
                info.Item()
                    .Text($"Material: {detalle.OrdenCompraDetalle!.Material.NombreMaterial}")
                    .Bold()
                    .FontSize(12);

                // Color
                info.Item()
                    .Text($"Color: {detalle.OrdenCompraDetalle.Material.Color}")
                    .FontSize(10);

                // Fecha
                info.Item()
                    .Text($"Fecha: {recepcion.FechaRecepcion:dd/MM/yyyy}")
                    .FontSize(10);

                // Bulto
                info.Item()
                    .Text($"Bulto: {detalle.BultosRecibidos:0.##}/{detalle.OrdenCompraDetalle?.Bultos:0.##}")
                    .Bold()
                    .FontSize(10);

                // Peso
                info.Item()
                    .Text($"Peso: {detalle.CantidadRecibida:0.##} Kg")
                    .Bold()
                    .FontSize(10);
            });
    }
}