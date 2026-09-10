using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using SistemaFlota.Models.Costos.OrdenesCompras;
using SistemaFlota.Services.Pdf.Styles;
using System.Globalization;

namespace SistemaFlota.Services.Pdf.Components;

public static class TablaCorporativa
{
    public static void Dibujar(
        IContainer container,
        IEnumerable<OrdenCompraDetalle> detalles)
    {
        container.Column(col =>
        {
            col.Item()
                .PaddingBottom(8)
                .Text("MATERIALES")
                .Style(PdfStyles.Subtitulo);

            col.Item()
                .Border(1)
                .BorderColor(PdfColors.GrisClaro)
                .CornerRadius(6)
                .Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(3);
                        columns.ConstantColumn(55);
                        columns.ConstantColumn(65);
                        columns.ConstantColumn(60);
                        columns.ConstantColumn(80);
                        columns.ConstantColumn(90);
                    });

                    table.Header(header =>
                    {
                        header.Cell().Element(HeaderCell)
                            .Text("Material")
                            .Style(PdfStyles.HeaderTabla);

                        header.Cell().Element(HeaderCell)
                            .AlignCenter()
                            .Text("KG")
                            .Style(PdfStyles.HeaderTabla);

                        header.Cell().Element(HeaderCell)
                            .AlignCenter()
                            .Text("KG/Bulto")
                            .Style(PdfStyles.HeaderTabla);

                        header.Cell().Element(HeaderCell)
                            .AlignCenter()
                            .Text("Bultos")
                            .Style(PdfStyles.HeaderTabla);

                        header.Cell().Element(HeaderCell)
                            .AlignRight()
                            .Text("Costo/KG")
                            .Style(PdfStyles.HeaderTabla);

                        header.Cell().Element(HeaderCell)
                            .AlignRight()
                            .Text("Subtotal")
                            .Style(PdfStyles.HeaderTabla);
                    });

                foreach (var item in detalles)
                    {
                        table.Cell().Element(BodyCell)
                            .Text(item.Material?.DescripcionCompra ?? "-")
                            .Style(PdfStyles.CeldaTabla);

                        table.Cell().Element(BodyCell)
                            .AlignRight()
                            .Text(item.CantidadKg.ToString("N0"))
                            .Style(PdfStyles.CeldaTabla);

                        table.Cell().Element(BodyCell)
                            .AlignRight()
                            .Text(item.KgPorBulto.ToString("N0"))
                            .Style(PdfStyles.CeldaTabla);

                        table.Cell().Element(BodyCell)
                            .AlignRight()
                            .Text(item.Bultos.ToString("N0"))
                            .Style(PdfStyles.CeldaTabla);

                        table.Cell().Element(BodyCell)
                            .AlignRight()
                            .Text(item.CostoKg.ToString("C0", new CultureInfo("es-CO")))
                            .Style(PdfStyles.CeldaTabla);

                        table.Cell().Element(BodyCell)
                            .AlignRight()
                            .Text(item.Subtotal.ToString("C0", new CultureInfo("es-CO")))
                            .Style(PdfStyles.CeldaTabla);
                    }

                });
        });
    }

    public static IContainer HeaderCell(IContainer container)
    {
        return container
            .Background(PdfColors.VerdePrincipal)
            .BorderBottom(1)
            .BorderColor(PdfColors.GrisClaro)
            .PaddingVertical(6)
            .PaddingHorizontal(4)
            .AlignCenter()
            .AlignMiddle();
    }

    public static IContainer BodyCell(IContainer container)
    {
        return container
            .BorderBottom(1)
            .BorderColor(PdfColors.GrisClaro)
            .PaddingVertical(5)
            .PaddingHorizontal(4);
    }
}