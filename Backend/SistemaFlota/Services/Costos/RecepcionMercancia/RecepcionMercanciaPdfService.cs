using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using SistemaFlota.Services.Pdf.Components;
using SistemaFlota.Services.Pdf.Styles;

namespace SistemaFlota.Services.Pdf.RecepcionMercancia;

public class RecepcionMercanciaPdfService : IRecepcionMercanciaPdfService
{
    private readonly AppDbContext _context;
    private readonly IWebHostEnvironment _environment;

    public RecepcionMercanciaPdfService(AppDbContext context, IWebHostEnvironment environment)
    {
        _context = context;
        _environment = environment;
    }

    public async Task<byte[]> GenerarPdfAsync(int idRecepcion)
    {
        var recepcion = await _context.RecepcionesMercancias
            .AsNoTracking()
            .Include(r => r.OrdenCompra)
                .ThenInclude(o => o.Proveedor)
            .Include(r => r.Detalles)
                .ThenInclude(d => d.OrdenCompraDetalle)
                    .ThenInclude(od => od.Material)
            .FirstOrDefaultAsync(r => r.Id == idRecepcion);

        if (recepcion == null)
            throw new Exception("La recepción de mercancía no existe.");

        var empresa = await _context.ConfiguracionEmpresa
            .AsNoTracking()
            .FirstOrDefaultAsync();

        if (empresa == null)
            throw new Exception("No existe la configuración de la empresa.");

        var documento = Document.Create(document =>
        {
            document.Page(page =>
            {
                page.Size(QuestPDF.Helpers.PageSizes.A4);
                page.Margin(25);
                page.DefaultTextStyle(x => x.FontSize(9));
                ConstruirDocumento(page, recepcion, empresa);
            });
        });

        return documento.GeneratePdf();
    }

    private void ConstruirDocumento(PageDescriptor page, Models.Costos.RecepcionMercancias.RecepcionMercancia recepcion, ConfiguracionEmpresa empresa)
    {
        page.Content().PaddingVertical(12).Column(col =>
        {
            col.Spacing(10);

            col.Item().Element(x =>
                HeaderEmpresa.Dibujar(
                    x,
                    ObtenerLogo(),
                    empresa,
                    "RECEPCIÓN DE MERCANCÍA",
                    "F-GC-009 V2",
                    "30/07/2026",
                    recepcion.NumeroRecepcion));

            // NOTA INICIAL
            col.Item()
                .Background(PdfColors.GrisClaro)
                .Border(1)
                .BorderColor(PdfColors.GrisClaro)
                .CornerRadius(5)
                .Padding(10)
                .Column(nota =>
                {
                    nota.Item()
                        .Text("NOTA")
                        .Bold()
                        .FontSize(9)
                        .FontColor(PdfColors.VerdePrincipal);

                    nota.Item()
                        .PaddingTop(3)
                        .Text("Las verificaciones consisten en comprobar la documentacion, condiciones de transporte, caracteristicas físicas de la materia prima, con los criterios de calidad definidos en el procedimiento de recepcion de materias primas.")
                        .FontSize(8)
                        .FontColor(PdfColors.AzulOscuro);
                });

            DibujarDatosRecepcion(col, recepcion);
            DibujarDatosOrden(col, recepcion);
            DibujarTransporte(col, recepcion);
            DibujarEntregas(col, recepcion);
            DibujarResumen(col, recepcion);

            // NOTA FINAL
            col.Item()
                .Background(PdfColors.GrisClaro)
                .Border(1)
                .BorderColor(PdfColors.GrisClaro)
                .CornerRadius(5)
                .Padding(10)
                .Column(nota =>
                {
                    nota.Item()
                        .Text("NOTA")
                        .Bold()
                        .FontSize(9)
                        .FontColor(PdfColors.VerdePrincipal);

                    nota.Item()
                        .PaddingTop(3)
                        .Text(text =>
                        {
                            text.Span("Los métodos de verificación de las características y los criterios de cumplimiento a tener en cuenta, están definidos en el ")
                                .FontSize(8)
                                .FontColor(PdfColors.AzulOscuro);

                            text.Span("\"procedimiento de Pruebas y Ensayos\"")
                                .Bold()
                                .FontSize(8)
                                .FontColor(PdfColors.AzulOscuro);

                            text.Span(" e ")
                                .FontSize(8)
                                .FontColor(PdfColors.AzulOscuro);

                            text.Span("\"Instructivos para Pruebas y Ensayos\"")
                                .Bold()
                                .FontSize(8)
                                .FontColor(PdfColors.AzulOscuro);

                            text.Span(".")
                                .FontSize(8)
                                .FontColor(PdfColors.AzulOscuro);
                        });
                });
        });

        page.Footer().Element(FooterEmpresa.Dibujar);
    }

    private void DibujarDatosRecepcion(ColumnDescriptor col, Models.Costos.RecepcionMercancias.RecepcionMercancia recepcion)
    {
        col.Item().Element(x =>
        {
            Card.Dibujar(x, "DATOS DE LA RECEPCIÓN", contenido =>
            {
                contenido.Item().Row(row =>
                {
                    row.RelativeItem().Column(c =>
                    {
                        c.Item().Text("Número de recepción").Style(PdfStyles.Label);
                        c.Item().Text(recepcion.NumeroRecepcion).Style(PdfStyles.Valor);
                    });

                    row.RelativeItem().Column(c =>
                    {
                        c.Item().Text("Fecha de recepción").Style(PdfStyles.Label);
                        c.Item().Text(recepcion.FechaRecepcion.ToString("dd/MM/yyyy HH:mm")).Style(PdfStyles.Valor);
                    });
                });

                contenido.Item().PaddingTop(8).Row(row =>
                {
                    row.RelativeItem().Column(c =>
                    {
                        c.Item().Text("Recibe").Style(PdfStyles.Label);
                        c.Item().Text(recepcion.Recibe).Style(PdfStyles.Valor);
                    });

                    row.RelativeItem().Column(c =>
                    {
                        c.Item().Text("Cargo").Style(PdfStyles.Label);
                        c.Item().Text(recepcion.Cargo).Style(PdfStyles.Valor);
                    });
                });
            });
        });
    }

    private void DibujarDatosOrden(ColumnDescriptor col, Models.Costos.RecepcionMercancias.RecepcionMercancia recepcion)
    {
        col.Item().Element(x =>
        {
            Card.Dibujar(x, "ORDEN DE COMPRA", contenido =>
            {
                contenido.Item().Row(row =>
                {
                    row.RelativeItem().Column(c =>
                    {
                        c.Item().Text("Número de orden").Style(PdfStyles.Label);
                        c.Item().Text(recepcion.OrdenCompra?.Numero ?? "-").Style(PdfStyles.Valor);
                    });

                    row.RelativeItem().Column(c =>
                    {
                        c.Item().Text("Proveedor").Style(PdfStyles.Label);
                        c.Item().Text(recepcion.OrdenCompra?.Proveedor?.Nombre ?? "-").Style(PdfStyles.Valor);
                    });
                });

                contenido.Item().PaddingTop(8).Row(row =>
                {
                    row.RelativeItem().Column(c =>
                    {
                        c.Item().Text("Fecha de orden").Style(PdfStyles.Label);
                        c.Item().Text(recepcion.OrdenCompra?.FechaOrden.ToString("dd/MM/yyyy") ?? "-").Style(PdfStyles.Valor);
                    });

                    row.RelativeItem().Column(c =>
                    {
                        c.Item().Text("Fecha de entrega").Style(PdfStyles.Label);
                        c.Item().Text(recepcion.OrdenCompra?.FechaEntrega?.ToString("dd/MM/yyyy") ?? "-").Style(PdfStyles.Valor);
                    });
                });

                contenido.Item().PaddingTop(8).Column(c =>
                {
                    c.Item().Text("Estado de la orden").Style(PdfStyles.Label);
                    c.Item().Text(recepcion.OrdenCompra?.Estado ?? "-").Style(PdfStyles.Valor);
                });
            });
        });
    }

    private void DibujarTransporte(ColumnDescriptor col, Models.Costos.RecepcionMercancias.RecepcionMercancia recepcion)
    {
        col.Item().Element(x =>
        {
            Card.Dibujar(x, "TRANSPORTE", contenido =>
            {
                contenido.Item().Row(row =>
                {
                    row.RelativeItem().Column(c =>
                    {
                        c.Item().Text("Conductor").Style(PdfStyles.Label);
                        c.Item().Text(recepcion.Conductor).Style(PdfStyles.Valor);
                    });

                    row.RelativeItem().Column(c =>
                    {
                        c.Item().Text("Transportadora").Style(PdfStyles.Label);
                        c.Item().Text(recepcion.Transportadora).Style(PdfStyles.Valor);
                    });
                });

                contenido.Item().PaddingTop(8).Row(row =>
                {
                    row.RelativeItem().Column(c =>
                    {
                        c.Item().Text("Tipo de documento").Style(PdfStyles.Label);
                        c.Item().Text(recepcion.TipoDocumento).Style(PdfStyles.Valor);
                    });

                    row.RelativeItem().Column(c =>
                    {
                        c.Item().Text("Embalaje adecuado").Style(PdfStyles.Label);
                        c.Item().Text(recepcion.EmbalajeAdecuado ? "Sí" : "No").Style(PdfStyles.Valor);
                    });
                });
            });
        });
    }

    private void DibujarEntregas(ColumnDescriptor col, Models.Costos.RecepcionMercancias.RecepcionMercancia recepcion)
    {
        var entregas = recepcion.Detalles
            .GroupBy(d => d.NumeroEntrega)
            .OrderBy(g => g.Key)
            .ToList();

        col.Item().Text("HISTORIAL DE ENTREGAS").Style(PdfStyles.Subtitulo);

        foreach (var entrega in entregas)
        {
            var fecha = entrega.Min(x => x.FechaEntrega);
            var totalKg = entrega.Sum(x => x.CantidadRecibida);
            var totalBultos = entrega.Sum(x => x.BultosRecibidos);
            var procesada = entrega.All(x => x.ProcesadoInventario);

            col.Item().Element(x =>
            {
                Card.Dibujar(x, $"ENTREGA #{entrega.Key}", contenido =>
                {
                    contenido.Item().Row(row =>
                    {
                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Text("Fecha de entrega").Style(PdfStyles.Label);
                            c.Item().Text(fecha.ToString("dd/MM/yyyy HH:mm")).Style(PdfStyles.Valor);
                        });

                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Text("Estado").Style(PdfStyles.Label);
                            c.Item().Text(procesada ? "Ingresada a inventario" : "Pendiente de inventario")
                                .Style(PdfStyles.Valor);
                        });
                    });

                    contenido.Item().PaddingTop(10).Element(tabla =>
                        TablaRecepcionMercancia.Dibujar(tabla, entrega));

                    contenido.Item().PaddingTop(8).Row(row =>
                    {
                        row.RelativeItem().Text($"Total KG: {totalKg:N2}").Style(PdfStyles.Valor);
                        row.RelativeItem().Text($"Total Bultos: {totalBultos:N2}").Style(PdfStyles.Valor);
                    });
                });
            });
        }
    }

    private void DibujarResumen(ColumnDescriptor col, Models.Costos.RecepcionMercancias.RecepcionMercancia recepcion)
    {
        var totalKg = recepcion.Detalles.Sum(x => x.CantidadRecibida);
        var totalBultos = recepcion.Detalles.Sum(x => x.BultosRecibidos);
        var totalEntregas = recepcion.Detalles.Select(x => x.NumeroEntrega).Distinct().Count();

        col.Item().Element(x =>
        {
            Card.Dibujar(x, "RESUMEN DE LA RECEPCIÓN", contenido =>
            {
                contenido.Item().Row(row =>
                {
                    row.RelativeItem().Column(c =>
                    {
                        c.Item().Text("Entregas").Style(PdfStyles.Label);
                        c.Item().Text(totalEntregas.ToString()).Style(PdfStyles.Total);
                    });

                    row.RelativeItem().Column(c =>
                    {
                        c.Item().Text("Total KG").Style(PdfStyles.Label);
                        c.Item().Text(totalKg.ToString("N2")).Style(PdfStyles.Total);
                    });

                    row.RelativeItem().Column(c =>
                    {
                        c.Item().Text("Total Bultos").Style(PdfStyles.Label);
                        c.Item().Text(totalBultos.ToString("N2")).Style(PdfStyles.Total);
                    });
                });

                contenido.Item().PaddingTop(10).LineHorizontal(1).LineColor(PdfColors.GrisClaro);

                contenido.Item().PaddingTop(10).Row(row =>
                {
                    row.RelativeItem().Column(c =>
                    {
                        c.Item().Text("Subtotal").Style(PdfStyles.Label);
                        c.Item().Text($"${recepcion.OrdenCompra?.Subtotal:N2}").Style(PdfStyles.Valor);
                    });

                    row.RelativeItem().Column(c =>
                    {
                        c.Item().Text("Impuesto").Style(PdfStyles.Label);
                        c.Item().Text($"${recepcion.OrdenCompra?.ValorImpuesto:N2}").Style(PdfStyles.Valor);
                    });

                    row.RelativeItem().Column(c =>
                    {
                        c.Item().Text("Total").Style(PdfStyles.Label);
                        c.Item().Text($"${recepcion.OrdenCompra?.TotalPagar:N2}").Style(PdfStyles.Total);
                    });
                });
            });
        });
    }

    private string ObtenerLogo()
    {
        return Path.Combine(_environment.ContentRootPath, "wwwroot", "config", "logo.png");
    }
}