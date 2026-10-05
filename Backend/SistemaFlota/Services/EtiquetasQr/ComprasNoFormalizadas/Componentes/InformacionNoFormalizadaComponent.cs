using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using SistemaFlota.Models.ComprasNoFormalizadas.RecepcionMercancias;

namespace SistemaFlota.Services.EtiquetasQr.ComprasNoFormalizadas.Componentes
{
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

                    // Proveedor
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

                    // Material
                    info.Item()
                        .Text(
                            $"Material: {detalle
                                .OrdenCompraDetalleNoFormalizada!
                                .MaterialNoFormalizado!
                                .NombreMaterial}")
                        .Bold()
                        .FontSize(12);

                    // Color
                    info.Item()
                        .Text(
                            $"Color: {detalle
                                .OrdenCompraDetalleNoFormalizada
                                .MaterialNoFormalizado
                                .Color}")
                        .FontSize(10);

                    // Fecha
                    info.Item()
                        .Text(
                            $"Fecha: {recepcion.FechaRecepcion:dd/MM/yyyy}")
                        .FontSize(10);

                    // Bulto
                    info.Item()
                        .Text($"Bulto: {numeroBulto}/{totalBultos}")
                        .Bold()
                        .FontSize(10);

                    // Peso
                    var pesoPorBulto = totalBultos > 0
                        ? detalle.CantidadRecibida / totalBultos
                        : 0;

                    info.Item()
                        .Text($"Peso: {pesoPorBulto:0.##} Kg")
                        .Bold()
                        .FontSize(10);
                });
        }
    }
}