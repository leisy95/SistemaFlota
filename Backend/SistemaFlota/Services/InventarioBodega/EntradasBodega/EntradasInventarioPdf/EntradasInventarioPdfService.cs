
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SistemaFlota.DTOs.InventarioBodega.EntradasBodega;
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

        // GENERAR PDF DE ENTRADAS DE INVENTARIO
        public async Task<byte[]> GenerarPdfAsync(
            string? buscar = null,
            DateTime? fechaInicio = null,
            DateTime? fechaFin = null)
        {
            // VALIDAR RANGO DE FECHAS
            if (fechaInicio.HasValue &&
                fechaFin.HasValue &&
                fechaInicio.Value.Date > fechaFin.Value.Date)
            {
                throw new ArgumentException(
                    "La fecha inicial no puede ser mayor que la fecha final.");
            }

            // CONSULTAR CONFIGURACIÓN DE EMPRESA
            var empresa = await _context.ConfiguracionEmpresa
                .FirstOrDefaultAsync();

            if (empresa == null)
                throw new Exception("No existe la configuración de la empresa.");

            // CONSULTAR ENTRADAS
            var query = _context.InventariosBodegaItems
                .AsNoTracking()
                .AsQueryable();

            // FILTRO POR REFERENCIA, REFERENCIA PESO Y ESTADO
            if (!string.IsNullOrWhiteSpace(buscar))
            {
                var texto = buscar.Trim();

                query = query.Where(i =>
                    i.InventarioBodega.Referencia.Contains(texto) ||
                    i.ReferenciaPeso.Contains(texto) ||
                    i.Estado.Contains(texto));
            }

            // FILTRO POR FECHA INICIAL
            if (fechaInicio.HasValue)
            {
                var inicio = fechaInicio.Value.Date;

                query = query.Where(i =>
                    i.FechaRegistroItem >= inicio);
            }

            // FILTRO POR FECHA FINAL
            // INCLUYE TODO EL DÍA SELECCIONADO
            if (fechaFin.HasValue)
            {
                var fin = fechaFin.Value.Date.AddDays(1);

                query = query.Where(i =>
                    i.FechaRegistroItem < fin);
            }

            // CONSULTAR TODOS LOS REGISTROS SIN PAGINACIÓN
            var entradas = await query
                .OrderByDescending(i => i.FechaRegistroItem)
                .ThenByDescending(i => i.Id)
                .Select(i => new MostrarEntradaDto
                {
                    Referencia = i.InventarioBodega.Referencia,
                    ReferenciaPeso = i.ReferenciaPeso,
                    FechaRegistroItem = i.FechaRegistroItem,
                    PesoActual = i.PesoActual,
                    Estado = i.Estado
                })
                .ToListAsync();

            if (!entradas.Any())
            {
                throw new InvalidOperationException(
                    "No se encontraron entradas de inventario para generar el PDF.");
            }

            // TOTALES
            var totalRegistros = entradas.Count;
            var pesoTotal = entradas.Sum(e => e.PesoActual);

            var cultura = CultureInfo.GetCultureInfo("es-CO");

            // GENERAR DOCUMENTO
            var pdf = Document.Create(document =>
            {
                document.Page(page =>
                {
                    // FORMATO A4 VERTICAL
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

        // RUTA DE MARCA DE AGUA
        private string ObtenerMarcaAgua()
        {
            return Path.Combine(
                _environment.ContentRootPath,
                "wwwroot",
                "config",
                "iguana3.png");
        }

        // RUTA DEL LOGO
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
            List<MostrarEntradaDto> entradas,
            ConfiguracionEmpresa empresa,
            string? buscar,
            DateTime? fechaInicio,
            DateTime? fechaFin,
            int totalRegistros,
            decimal pesoTotal,
            CultureInfo cultura)
        {
            // MARCA DE AGUA
            page.Background()
                .Element(container =>
                    MarcaAgua.Dibujar(
                        container,
                        ObtenerMarcaAgua()));

            // CONTENIDO
            page.Content()
                .Column(col =>
                {
                    col.Spacing(12);

                    // ENCABEZADO CORPORATIVO
                    // APARECE SOLAMENTE UNA VEZ
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
                                        text.Span("Fecha de generación: ")
                                            .SemiBold();

                                        text.Span(
                                            DateTime.Now.ToString("dd/MM/yyyy HH:mm"));
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

                    // RESUMEN
                    col.Item().Row(row =>
                    {
                        // TOTAL ENTRADAS
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

                        // PESO TOTAL
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

                    // TABLA DE ENTRADAS
                    col.Item().Element(container =>
                    {
                        Card.Dibujar(
                            container,
                            "DETALLE DE ENTRADAS DE INVENTARIO",
                            contenido =>
                            {
                                contenido.Item().Table(table =>
                                {
                                    // DISTRIBUCIÓN DE COLUMNAS
                                    table.ColumnsDefinition(columns =>
                                    {
                                        columns.RelativeColumn(2.2f); // REFERENCIA
                                        columns.RelativeColumn(1.6f); // REF. PESO
                                        columns.RelativeColumn(1.5f); // FECHA
                                        columns.RelativeColumn(0.9f); // PESO
                                        columns.RelativeColumn(1.3f); // ESTADO
                                    });

                                    // ENCABEZADO DE TABLA
                                    table.Header(header =>
                                    {
                                        CrearEncabezado(
                                            header.Cell(), "REFERENCIA");

                                        CrearEncabezado(
                                            header.Cell(), "REF. PESO");

                                        CrearEncabezado(
                                            header.Cell(), "FECHA");

                                        CrearEncabezado(
                                            header.Cell(), "PESO");

                                        CrearEncabezado(
                                            header.Cell(), "ESTADO");
                                    });

                                    // REGISTROS
                                    foreach (var entrada in entradas)
                                    {
                                        // REFERENCIA
                                        CrearCelda(
                                            table.Cell(),
                                            entrada.Referencia ?? "");

                                        // REFERENCIA PESO
                                        CrearCelda(
                                            table.Cell(),
                                            entrada.ReferenciaPeso ?? "");

                                        // FECHA
                                        CrearCeldaCentrada(
                                            table.Cell(),
                                            entrada.FechaRegistroItem.ToString(
                                                "dd/MM/yyyy HH:mm"));

                                        // PESO
                                        CrearCeldaCentrada(
                                            table.Cell(),
                                            $"{entrada.PesoActual.ToString("N2", cultura)} kg");

                                        // ESTADO
                                        CrearCeldaCentrada(
                                            table.Cell(),
                                            entrada.Estado ?? "");
                                    }
                                });
                            });
                    });

                    // TOTAL FINAL
                    col.Item()
                        .PaddingTop(10)
                        .AlignRight()
                        .Text(
                            $"TOTAL PESO: {pesoTotal.ToString("N2", cultura)} kg")
                        .Style(PdfStyles.Total);
                });

            // PIE DE PÁGINA
            page.Footer()
                .Element(container =>
                    FooterEmpresa.Dibujar(
                        container,
                        "SistemaFlota - Entradas de Inventario"));
        }

        // CREAR ENCABEZADOS DE TABLA
        private static void CrearEncabezado(
            IContainer container,
            string texto)
        {
            container
                .Background(PdfColors.VerdeOscuro)
                .Padding(5)
                .AlignCenter()
                .AlignMiddle()
                .Text(texto)
                .Style(PdfStyles.HeaderTabla);
        }

        // CREAR CELDAS ALINEADAS A LA IZQUIERDA
        private static void CrearCelda(
            IContainer container,
            string texto)
        {
            container
                .BorderBottom(1)
                .BorderColor(PdfColors.GrisClaro)
                .PaddingVertical(8)
                .PaddingHorizontal(6)
                .AlignMiddle()
                .Text(texto)
                .Style(PdfStyles.CeldaTabla)
                .FontSize(8);
        }

        // CREAR CELDAS CENTRADAS
        private static void CrearCeldaCentrada(
            IContainer container,
            string texto)
        {
            container
                .BorderBottom(1)
                .BorderColor(PdfColors.GrisClaro)
                .PaddingVertical(8)
                .PaddingHorizontal(4)
                .AlignCenter()
                .AlignMiddle()
                .Text(texto)
                .Style(PdfStyles.CeldaTabla)
                .FontSize(8);
        }
    }
}
