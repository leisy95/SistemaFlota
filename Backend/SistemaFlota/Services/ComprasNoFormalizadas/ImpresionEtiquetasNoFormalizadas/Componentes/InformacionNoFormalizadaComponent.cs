using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SistemaFlota.Models.ComprasNoFormalizadas.RecepcionMercancias;

namespace SistemaFlota.Services.ComprasNoFormalizadas.ImpresionEtiquetasNoFormalizadas.Componentes;

public static class InformacionNoFormalizadaComponent
{
    public static void Dibujar(
        IContainer container,
        RecepcionMercanciaNoFormalizada recepcion,
        RecepcionMercanciaDetalleNoFormalizada detalle,
        int numeroBulto,
        int totalBultos)
    {
        container
            .AlignMiddle()
            .Column(info =>
            {
                info.Spacing(1);

                info.Item()
                    .Text(
                        recepcion
                            .OrdenCompraNoFormalizada!
                            .ProveedorNoFormalizado!
                            .Nombre)
                    .Bold()
                    .FontSize(14);

                info.Item()
                    .LineHorizontal(0.5f);

                info.Item()
                    .Text(
                        $"Material: {detalle.OrdenCompraDetalleNoFormalizada!.MaterialNoFormalizado!.NombreMaterial}")
                    .Bold()
                    .FontSize(12);

                info.Item()
                    .Text(
                        $"Color: {detalle.OrdenCompraDetalleNoFormalizada.MaterialNoFormalizado.Color}")
                    .FontSize(10);

                info.Item()
                    .Text(
                        $"Fecha: {recepcion.FechaRecepcion:dd/MM/yyyy}")
                    .FontSize(10);

                info.Item()
                    .Text(
                        $"Bulto: {numeroBulto}/{totalBultos}")
                    .Bold()
                    .FontSize(10);

                var pesoPorBulto = totalBultos > 0
                    ? detalle.CantidadRecibida / totalBultos
                    : 0;

                info.Item()
                    .Text(
                        $"Peso: {pesoPorBulto:0.##} Kg")
                    .Bold()
                    .FontSize(10);
            });
    }
}