
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SistemaFlota.Services.Pdf.Components;
using SistemaFlota.Services.Pdf.Styles;
using System.Globalization;

namespace SistemaFlota.Services.InventarioBodega.EntradasBodega.EntradasInventarioPdf
{
    public class EntradasInventarioPdfService : IEntradasInventarioPdfService
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public EntradasInventarioPdfService(
            AppDbContext context,
            IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        // GENERAR PDF
        public async Task<byte[]> GenerarPdfAsync(
            string? buscar = null,
            DateTime? fechaInicio = null,
            DateTime? fechaFin = null)
        {
            if (fechaInicio.HasValue &&
                fechaFin.HasValue &&
                fechaInicio.Value.Date > fechaFin.Value.Date)
            {
                throw new ArgumentException(
                    "La fecha inicial no puede ser mayor que la fecha final.");
            }

            // CONFIGURACIÓN EMPRESA
            var empresa = await _context.ConfiguracionEmpresa
                .FirstOrDefaultAsync();

            if (empresa == null)
                throw new Exception("No existe la configuración de la empresa.");

            // CONSULTAR ENTRADAS
            var query = _context.InventariosBodegaItems
                .AsNoTracking()
                .AsQueryable();

            // BÚSQUEDA
            if (!string.IsNullOrWhiteSpace(buscar))
            {
                var texto = buscar.Trim();

                query = query.Where(i =>
                    i.InventarioBodega.Referencia.Contains(texto) ||
                    i.ReferenciaPeso.Contains(texto) ||
                    i.Estado.Contains(texto) ||
                    (i.InventarioBodega.Descripcion != null &&
                     i.InventarioBodega.Descripcion.Contains(texto)));
            }

            // FECHA INICIAL
            if (fechaInicio.HasValue)
            {
                var inicio = fechaInicio.Value.Date;

                query = query.Where(i =>
                    i.FechaRegistroItem >= inicio);
            }

            // FECHA FINAL
            if (fechaFin.HasValue)
            {
                var fin = fechaFin.Value.Date.AddDays(1);

                query = query.Where(i =>
                    i.FechaRegistroItem < fin);
            }

            // OBTENER DATOS DEL INVENTARIO Y PESOS INDIVIDUALES
            var datos = await query
                .OrderByDescending(i => i.FechaRegistroItem)
                .ThenByDescending(i => i.Id)
                .Select(i => new
                {
                    Referencia = i.InventarioBodega.Referencia,
                    Descripcion = i.InventarioBodega.Descripcion,
                    TipoBolsa = i.InventarioBodega.TipoBolsa,
                    TipoMaterial = i.InventarioBodega.TipoMaterial,
                    Densidad = i.InventarioBodega.Densidad,
                    Color = i.InventarioBodega.Color,
                    SegundoColor = i.InventarioBodega.SegundoColor,
                    ImpresoNo = i.InventarioBodega.ImpresoNo,
                    ReferenciaImpresa = i.InventarioBodega.ReferenciaImpresa,
                    Ancho = i.InventarioBodega.Ancho,
                    Alto = i.InventarioBodega.Alto,
                    Calibre = i.InventarioBodega.Calibre,
                    PesoActual = i.PesoActual,
                    FechaRegistroItem = i.FechaRegistroItem,
                    Estado = i.Estado
                })
                .ToListAsync();

            if (!datos.Any())
            {
                throw new InvalidOperationException(
                    "No se encontraron entradas de inventario para generar el PDF.");
            }

            // PREPARAR DATOS DEL PDF
            var entradas = datos.Select(i => new EntradaPdfDto
            {
                Referencia = i.Referencia,
                Descripcion = ConstruirDescripcion(
                    i.Descripcion,
                    i.TipoBolsa,
                    i.TipoMaterial,
                    i.Densidad,
                    i.Color,
                    i.SegundoColor,
                    i.ImpresoNo,
                    i.ReferenciaImpresa,
                    i.Ancho,
                    i.Alto,
                    i.Calibre),
                PesoActual = i.PesoActual,
                FechaRegistroItem = i.FechaRegistroItem,
                Estado = i.Estado
            }).ToList();

            // TOTALES GENERALES
            var totalRegistros = entradas.Count;
            var pesoTotal = entradas.Sum(e => e.PesoActual);

            var cultura = CultureInfo.GetCultureInfo("es-CO");

            // GENERAR DOCUMENTO
            var pdf = Document.Create(document =>
            {
                document.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(20);
                    page.DefaultTextStyle(x => x.FontSize(9));

                    ConstruirDocumento(
                        page,
                        entradas,
                        empresa,
                        buscar,
                        fechaInicio,
                        fechaFin,
                        totalRegistros,
                        pesoTotal,
                        cultura);
                });
            });

            return pdf.GeneratePdf();
        }

        // CONSTRUIR DESCRIPCIÓN DEL PRODUCTO
        private static string ConstruirDescripcion(
            string? descripcion,
            string? tipoBolsa,
            string? tipoMaterial,
            string? densidad,
            string? color,
            string? segundoColor,
            string? impresoNo,
            string? referenciaImpresa,
            decimal ancho,
            decimal alto,
            decimal calibre)
        {
            var partes = new List<string>();

            Agregar(parte: descripcion, partes: partes);
            Agregar(parte: tipoBolsa, partes: partes);
            Agregar(parte: color, partes: partes);
            Agregar(parte: segundoColor, partes: partes);
            Agregar(parte: densidad, partes: partes);
            Agregar(parte: tipoMaterial, partes: partes);

            if (!string.IsNullOrWhiteSpace(impresoNo))
                Agregar($"IMPRESIÓN: {impresoNo}", partes);

            Agregar(parte: referenciaImpresa, partes: partes);

            if (ancho > 0 || alto > 0)
            {
                Agregar(
                    $"MEDIDAS {ancho.ToString("0.##", CultureInfo.InvariantCulture)}X{alto.ToString("0.##", CultureInfo.InvariantCulture)}",
                    partes);
            }

            if (calibre > 0)
            {
                Agregar(
                    $"CAL. {calibre.ToString("0.##", CultureInfo.InvariantCulture)}",
                    partes);
            }

            return string.Join(" ", partes);
        }

        private static void Agregar(string? parte, List<string> partes)
        {
            if (!string.IsNullOrWhiteSpace(parte))
            {
                partes.Add(parte.Trim());
            }
        }

        // RUTA MARCA DE AGUA
        private string ObtenerMarcaAgua()
        {
            return Path.Combine(
                _environment.ContentRootPath,
                "wwwroot",
                "config",
                "iguana3.png");
        }

        // RUTA LOGO
        private string ObtenerLogo(string? logo)
        {
            return Path.Combine(
                _environment.ContentRootPath,
                "wwwroot",
                "config",
                "logo.png");
        }

        // CONSTRUIR DOCUMENTO
        private void ConstruirDocumento(
            PageDescriptor page,
            List<EntradaPdfDto> entradas,
            ConfiguracionEmpresa empresa,
            string? buscar,
            DateTime? fechaInicio,
            DateTime? fechaFin,
            int totalRegistros,
            decimal pesoTotal,
            CultureInfo cultura)
        {
            // AGRUPAR POR REFERENCIA
            var gruposEntradas = entradas
                .GroupBy(e => e.Referencia)
                .OrderBy(g => g.Key)
                .ToList();

            // MARCA DE AGUA
            page.Background()
                .Element(container =>
                    MarcaAgua.Dibujar(
                        container,
                        ObtenerMarcaAgua()));

            page.Content()
                .Column(col =>
                {
                    col.Spacing(12);

                    // ENCABEZADO CORPORATIVO
                    col.Item().Element(container =>
                    {
                        HeaderEmpresa.Dibujar(
                            container,
                            ObtenerLogo(empresa.Logo),
                            empresa,
                            "REPORTE DE ENTRADAS",
                            "F-GC-027 V2",
                            DateTime.Now.ToString("dd/MM/yyyy"),
                            "INVENTARIO BODEGA");
                    });

                    // INFORMACIÓN DEL REPORTE
                    col.Item()
                        .PaddingTop(10)
                        .Element(container =>
                        {
                            Card.Dibujar(
                                container,
                                "INFORMACIÓN DEL REPORTE",
                                contenido =>
                                {
                                    contenido.Spacing(5);

                                    contenido.Item().Text(text =>
                                    {
                                        text.Span("Fecha de generación: ").SemiBold();
                                        text.Span(DateTime.Now.ToString("dd/MM/yyyy HH:mm"));
                                    });

                                    contenido.Item().Text(text =>
                                    {
                                        text.Span("Búsqueda: ").SemiBold();
                                        text.Span(
                                            string.IsNullOrWhiteSpace(buscar)
                                                ? "Todos"
                                                : buscar);
                                    });

                                    contenido.Item().Text(text =>
                                    {
                                        text.Span("Fecha inicial: ").SemiBold();
                                        text.Span(
                                            fechaInicio?.ToString("dd/MM/yyyy")
                                            ?? "No definida");
                                    });

                                    contenido.Item().Text(text =>
                                    {
                                        text.Span("Fecha final: ").SemiBold();
                                        text.Span(
                                            fechaFin?.ToString("dd/MM/yyyy")
                                            ?? "No definida");
                                    });
                                });
                        });

                    // RESUMEN GENERAL
                    col.Item().Row(row =>
                    {
                        row.RelativeItem().Element(container =>
                        {
                            Card.Dibujar(
                                container,
                                "TOTAL ENTRADAS",
                                contenido =>
                                {
                                    contenido.Item()
                                        .Text(totalRegistros.ToString("N0", cultura))
                                        .Style(PdfStyles.Total);
                                });
                        });

                        row.ConstantItem(10);

                        row.RelativeItem().Element(container =>
                        {
                            Card.Dibujar(
                                container,
                                "PESO TOTAL",
                                contenido =>
                                {
                                    contenido.Item()
                                        .Text($"{pesoTotal.ToString("N2", cultura)} kg")
                                        .Style(PdfStyles.Total);
                                });
                        });
                    });

                    // DETALLE AGRUPADO POR REFERENCIA
                    col.Item().Element(container =>
                    {
                        Card.Dibujar(
                            container,
                            "DETALLE DE ENTRADAS DE INVENTARIO",
                            contenido =>
                            {
                                contenido.Spacing(12);

                                foreach (var grupo in gruposEntradas)
                                {
                                    contenido.Item().Column(seccion =>
                                    {
                                        var primerRegistro = grupo.First();

                                        // REFERENCIA
                                        seccion.Item()
                                            .Background("#E8F5E9")
                                            .BorderBottom(1)
                                            .BorderColor(PdfColors.VerdePrincipal)
                                            .Padding(8)
                                            .Text(text =>
                                            {
                                                text.Span("REFERENCIA: ")
                                                    .SemiBold()
                                                    .FontColor(PdfColors.VerdeOscuro);

                                                text.Span(grupo.Key ?? "")
                                                    .SemiBold()
                                                    .FontColor(PdfColors.AzulOscuro);
                                            });

                                        // DESCRIPCIÓN DEL PRODUCTO
                                        seccion.Item()
                                            .PaddingHorizontal(8)
                                            .PaddingTop(6)
                                            .Text(primerRegistro.Descripcion)
                                            .FontSize(8)
                                            .FontColor(PdfColors.GrisOscuro);

                                        // PESOS INDIVIDUALES
                                        seccion.Item()
                                            .Padding(8)
                                            .Table(table =>
                                            {
                                                const int columnas = 5;

                                                table.ColumnsDefinition(columns =>
                                                {
                                                    for (int i = 0; i < columnas; i++)
                                                    {
                                                        columns.RelativeColumn();
                                                    }
                                                });

                                                foreach (var entrada in grupo)
                                                {
                                                    table.Cell().Element(c =>
                                                        CrearCeldaPeso(
                                                            c,
                                                            entrada.PesoActual,
                                                            cultura));
                                                }
                                            });

                                        // TOTAL DE LA REFERENCIA
                                        var totalReferencia =
                                            grupo.Sum(e => e.PesoActual);

                                        seccion.Item()
                                            .PaddingTop(5)
                                            .BorderTop(1)
                                            .BorderColor(PdfColors.GrisClaro)
                                            .Padding(8)
                                            .AlignRight()
                                            .Text(text =>
                                            {
                                                text.Span("TOTAL DE LA REFERENCIA: ")
                                                    .SemiBold();

                                                text.Span(
                                                    $"{totalReferencia.ToString("N2", cultura)} kg")
                                                    .Bold()
                                                    .FontColor(PdfColors.VerdeOscuro);
                                            });
                                    });
                                }
                            });
                    });

                    // TOTAL GENERAL
                    col.Item()
                        .PaddingTop(10)
                        .AlignRight()
                        .Text(
                            $"TOTAL GENERAL: {pesoTotal.ToString("N2", cultura)} kg")
                        .Style(PdfStyles.Total);
                });

            // PIE DE PÁGINA
            page.Footer()
                .Element(container =>
                    FooterEmpresa.Dibujar(
                        container,
                        "SistemaFlota - Entradas de Inventario"));
        }

        // CELDA DE PESO INDIVIDUAL
        private static void CrearCeldaPeso(
            IContainer container,
            decimal peso,
            CultureInfo cultura)
        {
            container
                .Padding(4)
                .Border(1)
                .BorderColor(PdfColors.GrisClaro)
                .Background(PdfColors.Fondo)
                .PaddingVertical(10)
                .AlignCenter()
                .AlignMiddle()
                .Text($"{peso.ToString("N2", cultura)} kg")
                .FontSize(9)
                .SemiBold()
                .FontColor(PdfColors.AzulOscuro);
        }

        // MODELO INTERNO DEL PDF
        private class EntradaPdfDto
        {
            public string Referencia { get; set; } = string.Empty;
            public string Descripcion { get; set; } = string.Empty;
            public decimal PesoActual { get; set; }
            public DateTime FechaRegistroItem { get; set; }
            public string Estado { get; set; } = string.Empty;
        }
    }
}
