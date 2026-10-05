using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SistemaFlota.Models.ComprasNoFormalizadas.RecepcionMercancias;

namespace SistemaFlota.Services.Pdf.Components
{
    public static class TablaRecepcionMercanciaNoFormalizada
    {
        public static void Dibujar(
            IContainer container,
            ICollection<RecepcionMercanciaDetalleNoFormalizada> detalles)
        {
            container.Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(3);
                    columns.RelativeColumn(2);
                    columns.RelativeColumn(2);
                    columns.RelativeColumn(2);
                    columns.RelativeColumn(2);
                });

                // Encabezados
                table.Header(header =>
                {
                    header.Cell()
                        .Element(CeldaEncabezado)
                        .Text("Material");

                    header.Cell()
                        .Element(CeldaEncabezado)
                        .Text("Cantidad Kg");

                    header.Cell()
                        .Element(CeldaEncabezado)
                        .Text("Bultos");

                    header.Cell()
                        .Element(CeldaEncabezado)
                        .Text("Lote");

                    header.Cell()
                        .Element(CeldaEncabezado)
                        .Text("Estado");
                });

                // Detalles
                foreach (var detalle in detalles)
                {
                    table.Cell()
                        .Element(Celda)
                        .Text(
                            detalle
                                .OrdenCompraDetalleNoFormalizada?
                                .MaterialNoFormalizado?
                                .NombreMaterial
                            ?? "");

                    table.Cell()
                        .Element(Celda)
                        .AlignRight()
                        .Text(
                            $"{detalle.CantidadRecibida:N2}");

                    table.Cell()
                        .Element(Celda)
                        .AlignRight()
                        .Text(
                            $"{detalle.BultosRecibidos:N2}");

                    table.Cell()
                        .Element(Celda)
                        .Text(
                            detalle.LoteProveedor ?? "");

                    table.Cell()
                        .Element(Celda)
                        .Text(
                            detalle.EstadoMaterial ?? "");
                }
            });
        }

        private static IContainer CeldaEncabezado(
            IContainer container)
        {
            return container
                .Background(Colors.Grey.Lighten2)
                .Border(0.5f)
                .BorderColor(Colors.Grey.Medium)
                .Padding(5)
                .DefaultTextStyle(x =>
                    x.Bold()
                     .FontSize(9));
        }

        private static IContainer Celda(
            IContainer container)
        {
            return container
                .Border(0.5f)
                .BorderColor(Colors.Grey.Lighten2)
                .Padding(5)
                .DefaultTextStyle(x =>
                    x.FontSize(9));
        }
    }
}
