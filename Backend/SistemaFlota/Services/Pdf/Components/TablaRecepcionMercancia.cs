using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using SistemaFlota.Models.Costos.RecepcionMercancias;
using SistemaFlota.Services.Pdf.Styles;

namespace SistemaFlota.Services.Pdf.Components;

public static class TablaRecepcionMercancia
{
    public static void Dibujar(
    IContainer container,
    IEnumerable<RecepcionMercanciaDetalle> detalles)
    {
        container.Column(col =>
        {
            col.Item()
                .PaddingBottom(8)
                .Text("MATERIALES RECIBIDOS")
                .Style(PdfStyles.Subtitulo);

            col.Item()
                .Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(3); // Material
                        columns.RelativeColumn(1); // Color
                        columns.ConstantColumn(55); // KG
                        columns.ConstantColumn(55); // Bultos
                        columns.RelativeColumn(2); // Lote
                        columns.RelativeColumn(2); // Estado
                    });

                    table.Header(header =>
                    {
                        header.Cell().Element(HeaderCell)
                            .Text("Material")
                            .Style(PdfStyles.HeaderTabla);

                        header.Cell().Element(HeaderCell)
                            .Text("Color")
                            .Style(PdfStyles.HeaderTabla);

                        header.Cell().Element(HeaderCell)
                            .Text("KG")
                            .Style(PdfStyles.HeaderTabla);

                        header.Cell().Element(HeaderCell)
                            .Text("Bultos")
                            .Style(PdfStyles.HeaderTabla);

                        header.Cell().Element(HeaderCell)
                            .Text("Lote")
                            .Style(PdfStyles.HeaderTabla);

                        header.Cell().Element(HeaderCell)
                            .Text("Estado")
                            .Style(PdfStyles.HeaderTabla);
                    });

                    foreach (var item in detalles)
                    {
                        table.Cell().Element(BodyCell)
                            .Text(item.OrdenCompraDetalle?.Material?.NombreMaterial ?? "-")
                            .Style(PdfStyles.CeldaTabla);

                        table.Cell().Element(BodyCell)
                            .Text(item.OrdenCompraDetalle?.Color ?? "-")
                            .Style(PdfStyles.CeldaTabla);

                        table.Cell().Element(BodyCell)
                            .AlignRight()
                            .Text(item.CantidadRecibida.ToString("N2"))
                            .Style(PdfStyles.CeldaTabla);

                        table.Cell().Element(BodyCell)
                            .AlignRight()
                            .Text(item.BultosRecibidos.ToString("N2"))
                            .Style(PdfStyles.CeldaTabla);

                        table.Cell().Element(BodyCell)
                            .Text(item.LoteProveedor)
                            .Style(PdfStyles.CeldaTabla);

                        table.Cell().Element(BodyCell).Element(x =>
                        {
                            EstadoBadge.Dibujar(x, item.EstadoMaterial);
                        });
                    }
                });
        });
    }

    private static IContainer HeaderCell(IContainer container)
    {
        return container
            .Background(PdfColors.AzulOscuro)
            .Border(1)
            .BorderColor(PdfColors.GrisClaro)
            .PaddingVertical(6)
            .PaddingHorizontal(4)
            .AlignCenter()
            .AlignMiddle();
    }

    private static IContainer BodyCell(IContainer container)
    {
        return container
            .BorderBottom(1)
            .BorderColor(PdfColors.GrisClaro)
            .PaddingVertical(5)
            .PaddingHorizontal(4);
    }
}