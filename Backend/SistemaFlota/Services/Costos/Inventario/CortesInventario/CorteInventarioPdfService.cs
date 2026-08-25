using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SistemaFlota.Migrations;
using SistemaFlota.Services.Pdf.Components;
using SistemaFlota.Services.Pdf.Styles;

namespace SistemaFlota.Services.Costos.Inventario.CortesInventario
{
    public class CorteInventarioPdfService : ICorteInventarioPdfService
    {
        private readonly AppDbContext _context;

        public CorteInventarioPdfService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<byte[]> GenerarPdfAsync()
        {
            var inventarios = await _context.Inventarios
                .Include(x => x.Material)
                .ThenInclude(x => x.Proveedor)
                .AsNoTracking()
                .OrderBy(x => x.Material!.Proveedor!.Nombre)
                .ThenBy(x => x.Material!.NombreMaterial)
                .ThenBy(x => x.Color)
                .ToListAsync();

            var fecha = DateTime.Now;

            var configuracion = await _context.ConfiguracionEmpresa
                .AsNoTracking()
                .FirstOrDefaultAsync();

            if (configuracion == null)
                throw new Exception("No existe configuración de empresa.");

            var logo = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "imagenes", "logo.png");
            var marcaAgua = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "imagenes", "iguana.png");

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.Letter);
                    page.Margin(30);
                    page.DefaultTextStyle(x => x.FontSize(9));

                    page.Header().Element(header =>
                    {
                        HeaderEmpresa.Dibujar(header, logo, configuracion, "CORTE DE INVENTARIO", $"MES {fecha:MM/yyyy}");
                    });

                    page.Content().PaddingTop(15).Column(column =>
                    {
                        column.Item().Element(content =>
                        {
                            Card.Dibujar(content, $"HOJA DE CONTEO FÍSICO - {fecha:MMMM yyyy}".ToUpper(), col =>
                            {
                                col.Item().Text("Revise físicamente cada material y registre manualmente el peso encontrado en la columna \"Conteo físico\".").Style(PdfStyles.Label);

                                col.Item().PaddingTop(8).Text($"Fecha de conteo: ____________________    Responsable: ______________________________").Style(PdfStyles.Valor);

                                col.Item().PaddingTop(12).Element(table =>
                                {
                                    table.Table(t =>
                                    {
                                        t.ColumnsDefinition(columns =>
                                        {
                                            columns.RelativeColumn(1.7f);
                                            columns.RelativeColumn(2.5f);
                                            columns.RelativeColumn(1.2f);
                                            columns.ConstantColumn(65);
                                            columns.ConstantColumn(85);
                                            columns.ConstantColumn(70);
                                        });

                                        t.Header(header =>
                                        {
                                            header.Cell().Element(TablaCorporativa.HeaderCell).Text("Proveedor").Style(PdfStyles.HeaderTabla);
                                            header.Cell().Element(TablaCorporativa.HeaderCell).Text("Material").Style(PdfStyles.HeaderTabla);
                                            header.Cell().Element(TablaCorporativa.HeaderCell).Text("Color").Style(PdfStyles.HeaderTabla);
                                            header.Cell().Element(TablaCorporativa.HeaderCell).AlignCenter().Text("Sistema KG").Style(PdfStyles.HeaderTabla);
                                            header.Cell().Element(TablaCorporativa.HeaderCell).AlignCenter().Text("Conteo físico KG").Style(PdfStyles.HeaderTabla);
                                            header.Cell().Element(TablaCorporativa.HeaderCell).AlignCenter().Text("Diferencia").Style(PdfStyles.HeaderTabla);
                                        });

                                        foreach (var item in inventarios)
                                        {
                                            t.Cell().Element(TablaCorporativa.BodyCell).Text(item.Material?.Proveedor?.Nombre ?? "-").Style(PdfStyles.CeldaTabla);
                                            t.Cell().Element(TablaCorporativa.BodyCell).Text(item.Material?.NombreMaterial ?? "-").Style(PdfStyles.CeldaTabla);
                                            t.Cell().Element(TablaCorporativa.BodyCell).Text(item.Color ?? "-").Style(PdfStyles.CeldaTabla);
                                            t.Cell().Element(TablaCorporativa.BodyCell).AlignRight().Text(item.StockActual.ToString("N2")).Style(PdfStyles.CeldaTabla);
                                            t.Cell().Element(CeldaParaEscribir).Text("");
                                            t.Cell().Element(CeldaParaEscribir).Text("");
                                        }
                                    });
                                });
                            });
                        });

                        column.Item().PaddingTop(15).Element(observaciones =>
                        {
                            Card.Dibujar(observaciones, "OBSERVACIONES", col =>
                            {
                                col.Item().Height(45).BorderBottom(1).BorderColor(PdfColors.GrisClaro);
                                col.Item().Height(45).BorderBottom(1).BorderColor(PdfColors.GrisClaro);
                            });
                        });

                        column.Item().PaddingTop(20).Row(row =>
                        {
                            row.RelativeItem().Column(col =>
                            {
                                col.Item().AlignCenter().BorderBottom(1).Width(180).Height(25);
                                col.Item().PaddingTop(5).AlignCenter().Text("Responsable del conteo").Style(PdfStyles.Footer);
                            });

                            row.ConstantItem(40);

                            row.RelativeItem().Column(col =>
                            {
                                col.Item().AlignCenter().BorderBottom(1).Width(180).Height(25);
                                col.Item().PaddingTop(5).AlignCenter().Text("Firma").Style(PdfStyles.Footer);
                            });
                        });
                    });

                    page.Background().Element(background =>
                    {
                        MarcaAgua.Dibujar(background, marcaAgua);
                    });

                    page.Footer().Element(FooterEmpresa.Dibujar);
                });
            });

            return document.GeneratePdf();
        }

        private static IContainer CeldaParaEscribir(IContainer container)
        {
            return container
                .BorderBottom(1)
                .BorderColor(PdfColors.GrisClaro)
                .PaddingVertical(5)
                .PaddingHorizontal(4)
                .MinHeight(28);
        }
    }
}