using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SistemaFlota.Migrations;
using SistemaFlota.Services.Pdf.Components;
using SistemaFlota.Services.Pdf.Styles;

namespace SistemaFlota.Services.Costos.Inventario.CortesInventario;

public class CorteInventarioPdfService : ICorteInventarioPdfService
{
    private readonly AppDbContext _context;
    private readonly IWebHostEnvironment _environment;

    public CorteInventarioPdfService(AppDbContext context, IWebHostEnvironment environment)
    {
        _context = context;
        _environment = environment;
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

        var logo = ObtenerLogo();
        var marcaAgua = ObtenerMarcaAgua();
        var totalKgSistema = inventarios.Sum(x => x.StockActual);

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.Letter);
                page.Margin(30);
                page.DefaultTextStyle(x => x.FontSize(9));

                page.Content().Column(column =>
                {
                    // ENCABEZADO
                    column.Item().ShowOnce().Element(header =>
                    {
                        HeaderEmpresa.Dibujar(header, logo, configuracion, "","CORTE DE INVENTARIO", $"MES {fecha:MM/yyyy}");
                    });

                    // INFORMACIÓN DEL CONTEO
                    column.Item().PaddingTop(12).Element(info =>
                    {
                        Card.Dibujar(info, "INFORMACIÓN DEL CONTEO", col =>
                        {
                            col.Item().Row(row =>
                            {
                                row.RelativeItem().Column(c =>
                                {
                                    c.Item().Text("Fecha de conteo").Style(PdfStyles.Label);
                                    c.Item().PaddingTop(3).Text("____ / ____ / ______").Style(PdfStyles.Valor);
                                });

                                row.RelativeItem().Column(c =>
                                {
                                    c.Item().Text("Responsable").Style(PdfStyles.Label);
                                    c.Item().PaddingTop(3).Text("____________________________").Style(PdfStyles.Valor);
                                });

                                row.RelativeItem().Column(c =>
                                {
                                    c.Item().Text("Bodega / Área").Style(PdfStyles.Label);
                                    c.Item().PaddingTop(3).Text("____________________________").Style(PdfStyles.Valor);
                                });
                            });
                        });
                    });

                    // TABLA DE CONTEO
                    column.Item().PaddingTop(12).Element(content =>
                    {
                        Card.Dibujar(content, $"HOJA DE CONTEO FÍSICO - {fecha:MMMM yyyy}".ToUpper(), col =>
                        {
                            col.Item()
                                .Text("Revise físicamente cada material y registre manualmente el peso encontrado en la columna \"Conteo físico KG\".")
                                .Style(PdfStyles.Label);

                            col.Item().PaddingTop(10).Element(table =>
                            {
                                table.Table(t =>
                                {
                                    t.ColumnsDefinition(columns =>
                                    {
                                        columns.RelativeColumn(1.7f);
                                        columns.RelativeColumn(2.5f);
                                        columns.RelativeColumn(1.2f);
                                        columns.ConstantColumn(62);
                                        columns.ConstantColumn(82);
                                        columns.ConstantColumn(68);
                                    });

                                    t.Header(header =>
                                    {
                                        header.Cell().Element(TablaCorporativa.HeaderCell).Text("Proveedor").Style(PdfStyles.HeaderTabla);
                                        header.Cell().Element(TablaCorporativa.HeaderCell).Text("Material").Style(PdfStyles.HeaderTabla);
                                        header.Cell().Element(TablaCorporativa.HeaderCell).Text("Color").Style(PdfStyles.HeaderTabla);
                                        header.Cell().Element(TablaCorporativa.HeaderCell).AlignCenter().Text("Sistema\nKG").Style(PdfStyles.HeaderTabla);
                                        header.Cell().Element(TablaCorporativa.HeaderCell).AlignCenter().Text("Conteo físico\nKG").Style(PdfStyles.HeaderTabla);
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

                    // RESUMEN
                    column.Item().PaddingTop(12).Element(resumen =>
                    {
                        Card.Dibujar(resumen, "RESUMEN DEL CONTEO", col =>
                        {
                            col.Item().Row(row =>
                            {
                                row.RelativeItem().Column(c =>
                                {
                                    c.Item().Text("Total referencias").Style(PdfStyles.Label);
                                    c.Item().PaddingTop(3).Text($"{inventarios.Count:N0}").Style(PdfStyles.Valor);
                                });

                                row.RelativeItem().Column(c =>
                                {
                                    c.Item().Text("Total KG sistema").Style(PdfStyles.Label);
                                    c.Item().PaddingTop(3).Text(totalKgSistema.ToString("N2")).Style(PdfStyles.Valor);
                                });

                                row.RelativeItem().Column(c =>
                                {
                                    c.Item().Text("Total KG contados").Style(PdfStyles.Label);
                                    c.Item().PaddingTop(3).Text("________________").Style(PdfStyles.Valor);
                                });

                                row.RelativeItem().Column(c =>
                                {
                                    c.Item().Text("Diferencia total").Style(PdfStyles.Label);
                                    c.Item().PaddingTop(3).Text("________________").Style(PdfStyles.Valor);
                                });
                            });

                            col.Item().PaddingTop(10).Row(row =>
                            {
                                row.AutoItem().Text("Resultado:").Style(PdfStyles.Label);
                                row.ConstantItem(10);
                                row.AutoItem().Text("☐ Conforme").Style(PdfStyles.Valor);
                                row.ConstantItem(12);
                                row.AutoItem().Text("☐ Con diferencias").Style(PdfStyles.Valor);
                                row.ConstantItem(12);
                                row.AutoItem().Text("☐ Requiere revisión").Style(PdfStyles.Valor);
                            });
                        });
                    });

                    // OBSERVACIONES
                    column.Item().PaddingTop(12).Element(observaciones =>
                    {
                        Card.Dibujar(observaciones, "OBSERVACIONES", col =>
                        {
                            col.Item().Height(90).BorderBottom(1).BorderColor(PdfColors.GrisClaro);
                        });
                    });

                    // FIRMAS
                    column.Item().PaddingTop(18).Row(row =>
                    {
                        row.RelativeItem().Column(col =>
                        {
                            col.Item().AlignCenter().BorderBottom(1).Width(180).Height(25);
                            col.Item().PaddingTop(5).AlignCenter().Text("Responsable del conteo").Style(PdfStyles.Footer);
                            col.Item().PaddingTop(3).AlignCenter().Text("Nombre y firma").Style(PdfStyles.Label);
                        });

                        row.ConstantItem(40);

                        row.RelativeItem().Column(col =>
                        {
                            col.Item().AlignCenter().BorderBottom(1).Width(180).Height(25);
                            col.Item().PaddingTop(5).AlignCenter().Text("Responsable de revisión").Style(PdfStyles.Footer);
                            col.Item().PaddingTop(3).AlignCenter().Text("Nombre y firma").Style(PdfStyles.Label);
                        });
                    });
                });

                // MARCA DE AGUA
                page.Background().Element(background =>
                {
                    MarcaAgua.Dibujar(background, marcaAgua);
                });

                // PIE
                page.Footer().Element(FooterEmpresa.Dibujar);
            });
        });

        return document.GeneratePdf();
    }

    private string ObtenerLogo()
    {
        var ruta = Path.Combine(_environment.WebRootPath, "config", "logo.png");

        if (!File.Exists(ruta))
            throw new FileNotFoundException("No se encontró el logo de la empresa.", ruta);

        return ruta;
    }

    private string ObtenerMarcaAgua()
    {
        var ruta = Path.Combine(
            _environment.WebRootPath,
            "config",
            "iguana3.png"
        );

        if (!File.Exists(ruta))
            throw new FileNotFoundException("No se encontró la marca de agua.", ruta);

        return ruta;
    }

    private static IContainer CeldaParaEscribir(IContainer container)
    {
        return container
            .BorderBottom(1)
            .BorderColor(PdfColors.GrisClaro)
            .PaddingVertical(8)
            .PaddingHorizontal(4)
            .MinHeight(34);
    }
}