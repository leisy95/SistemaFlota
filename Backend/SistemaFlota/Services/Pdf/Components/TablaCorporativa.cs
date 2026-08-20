using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using SistemaFlota.Models.Costos.OrdenesCompras;
using SistemaFlota.Services.Pdf.Styles;

namespace SistemaFlota.Services.Pdf.Components;

public static class TablaCorporativa
{
    public static void Dibujar(
    IContainer container,
    IEnumerable<OrdenCompraDetalle> detalles)
    {
        container.Column(col =>
        {
            // Título
            col.Item()
                .PaddingBottom(8)
                .Text("MATERIALES")
                .Style(PdfStyles.Subtitulo);

            // Tabla
            col.Item()
                .Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(3);
                        columns.RelativeColumn(1);
                        columns.ConstantColumn(55);
                        columns.ConstantColumn(65);
                        columns.ConstantColumn(60);
                        columns.ConstantColumn(80);
                        columns.ConstantColumn(90);
                    });

                    table.Header(header =>
                    {
                        header.Cell().Element(HeaderCell).Text("Material").Style(PdfStyles.HeaderTabla);
                        header.Cell().Element(HeaderCell).Text("Color").Style(PdfStyles.HeaderTabla);
                        header.Cell().Element(HeaderCell).Text("KG").Style(PdfStyles.HeaderTabla);
                        header.Cell().Element(HeaderCell).Text("KG/Bulto").Style(PdfStyles.HeaderTabla);
                        header.Cell().Element(HeaderCell).Text("Bultos").Style(PdfStyles.HeaderTabla);
                        header.Cell().Element(HeaderCell).Text("Costo/KG").Style(PdfStyles.HeaderTabla);
                        header.Cell().Element(HeaderCell).Text("Subtotal").Style(PdfStyles.HeaderTabla);
                    });

                    // Filas
                    foreach (var item in detalles)
                    {
                        table.Cell().Element(BodyCell)
                            .Text(item.Material?.NombreMaterial ?? "-")
                            .Style(PdfStyles.CeldaTabla);

                        table.Cell().Element(BodyCell)
                            .Text(item.Color)
                            .Style(PdfStyles.CeldaTabla);

                        table.Cell().Element(BodyCell)
                            .AlignRight()
                            .Text(item.CantidadKg.ToString("N2"))
                            .Style(PdfStyles.CeldaTabla);

                        table.Cell().Element(BodyCell)
                            .AlignRight()
                            .Text(item.KgPorBulto.ToString("N2"))
                            .Style(PdfStyles.CeldaTabla);

                        table.Cell().Element(BodyCell)
                            .AlignRight()
                            .Text(item.Bultos.ToString("N2"))
                            .Style(PdfStyles.CeldaTabla);

                        table.Cell().Element(BodyCell)
                            .AlignRight()
                            .Text(item.CostoKg.ToString("C0"))
                            .Style(PdfStyles.CeldaTabla);

                        table.Cell().Element(BodyCell)
                            .AlignRight()
                            .Text(item.Subtotal.ToString("C0"))
                            .Style(PdfStyles.CeldaTabla);
                    }
                });
        });
    }

    public static IContainer HeaderCell(IContainer container)
    {
        return container
            .Background(PdfColors.VerdePrincipal)
            .Border(1)
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