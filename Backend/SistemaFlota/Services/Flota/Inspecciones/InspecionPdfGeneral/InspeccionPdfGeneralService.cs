
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SistemaFlota.Services.Pdf.Components;
using SistemaFlota.Services.Pdf.Styles;

namespace SistemaFlota.Services.Flota.Inspecciones.InspecionPdfGeneral
{
    public class InspeccionPdfGeneralService
        : IInspeccionPdfGeneralService
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public InspeccionPdfGeneralService(
            AppDbContext context,
            IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        public async Task<byte[]> GenerarPdfGeneralAsync(
            string? buscar = null,
            DateTime? fechaDesde = null,
            DateTime? fechaHasta = null)
        {
            // 1. Consultar inspecciones
            var query = _context.Inspecciones
                .AsNoTracking()
                .Include(i => i.Conductor)
                .Include(i => i.Vehiculo)
                .AsQueryable();

            // 2. Filtro por conductor o placa
            if (!string.IsNullOrWhiteSpace(buscar))
            {
                var termino = buscar.Trim().ToLower();

                query = query.Where(i =>
                    (i.Conductor != null &&
                     i.Conductor.Nombre.ToLower()
                         .Contains(termino))
                    ||
                    (i.Vehiculo != null &&
                     i.Vehiculo.Placa.ToLower()
                         .Contains(termino)));
            }

            // 3. Fecha inicial: inclusiva
            if (fechaDesde.HasValue)
            {
                var desde = fechaDesde.Value.Date;

                query = query.Where(i => i.Fecha >= desde);
            }

            // 4. Fecha final: incluye todo ese día
            if (fechaHasta.HasValue)
            {
                var hastaExclusiva =
                    fechaHasta.Value.Date.AddDays(1);

                query = query.Where(
                    i => i.Fecha < hastaExclusiva);
            }

            var inspecciones = await query
                .OrderByDescending(i => i.Fecha)
                .ToListAsync();

            if (inspecciones.Count == 0)
            {
                throw new InvalidOperationException(
                    "No hay inspecciones que coincidan con los filtros.");
            }

            // 5. Configuración de la empresa
            var empresa = await _context.ConfiguracionEmpresa
                .AsNoTracking()
                .FirstOrDefaultAsync();

            if (empresa == null)
            {
                throw new InvalidOperationException(
                    "No existe la configuración de la empresa.");
            }

            // 6. Recursos corporativos
            var rutaLogo = Path.Combine(
                _environment.ContentRootPath,
                "wwwroot", "config", "logo.png");

            var rutaMarcaAgua = Path.Combine(
                _environment.ContentRootPath,
                "wwwroot", "config", "iguana3.png");

            // 7. Generar documento
            var pdf = Document.Create(document =>
            {
                document.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(20);

                    page.DefaultTextStyle(x =>
                        x.FontSize(9));

                    // Marca de agua
                    page.Background()
                        .Element(container =>
                            MarcaAgua.Dibujar(
                                container,
                                rutaMarcaAgua));

                    // Contenido del reporte
                    page.Content()
                        .PaddingTop(20)
                        .Column(content =>
                        {
                            content.Spacing(12);

                            // Encabezado corporativo
                            content.Item()
                                .Element(container =>
                                {
                                    HeaderEmpresa.Dibujar(
                                        container,
                                        rutaLogo,
                                        empresa,
                                        "F-GC-027 V2",
                                        "",
                                        "HISTORIAL GENERAL DE INSPECCIONES",
                                        $"{inspecciones.Count} registros");
                                });

                            // Resumen
                            content.Item()
                                .Border(1)
                                .BorderColor(PdfColors.GrisClaro)
                                .Background(PdfColors.Fondo)
                                .Padding(10)
                                .Column(col =>
                                {
                                    col.Item()
                                        .Text("RESUMEN DEL REPORTE")
                                        .Style(PdfStyles.Subtitulo);

                                    col.Item()
                                        .Text(
                                            $"Total de inspecciones: " +
                                            $"{inspecciones.Count}")
                                        .Style(PdfStyles.Label);

                                    col.Item()
                                        .Text(
                                            $"Fecha de generación: " +
                                            $"{DateTime.Now:dd/MM/yyyy HH:mm}")
                                        .Style(PdfStyles.Label);

                                    col.Item()
                                        .Text(
                                            $"Búsqueda: " +
                                            $"{(string.IsNullOrWhiteSpace(buscar) ? "Todas" : buscar.Trim())}")
                                        .Style(PdfStyles.Label);

                                    col.Item()
                                        .Text(
                                            $"Desde: " +
                                            $"{(fechaDesde.HasValue ? fechaDesde.Value.ToString("dd/MM/yyyy") : "Sin límite")}")
                                        .Style(PdfStyles.Label);

                                    col.Item()
                                        .Text(
                                            $"Hasta: " +
                                            $"{(fechaHasta.HasValue ? fechaHasta.Value.ToString("dd/MM/yyyy") : "Sin límite")}")
                                        .Style(PdfStyles.Label);
                                });

                            // Tabla general
                            content.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.ConstantColumn(25);
                                    columns.RelativeColumn(1.5f);
                                    columns.RelativeColumn(2.5f);
                                    columns.RelativeColumn(1.4f);
                                    columns.RelativeColumn(1.3f);
                                    columns.RelativeColumn(1.5f);
                                });

                                table.Header(header =>
                                {
                                    CeldaCabecera(header, "#");
                                    CeldaCabecera(header, "Fecha");
                                    CeldaCabecera(header, "Conductor");
                                    CeldaCabecera(header, "Vehículo");
                                    CeldaCabecera(header, "Kilometraje");
                                    CeldaCabecera(header, "WhatsApp");
                                });

                                for (var i = 0; i < inspecciones.Count; i++)
                                {
                                    var item = inspecciones[i];

                                    CeldaCuerpo(
                                        table,
                                        (i + 1).ToString());

                                    CeldaCuerpo(
                                        table,
                                        item.Fecha.ToString("dd/MM/yyyy HH:mm"));

                                    CeldaCuerpo(
                                        table,
                                        item.Conductor?.Nombre
                                            ?? "No registrado");

                                    CeldaCuerpo(
                                        table,
                                        item.Vehiculo?.Placa
                                            ?? "No registrada");

                                    CeldaCuerpo(
                                        table,
                                        $"{item.Kilometraje:N0} km");

                                    CeldaWhatsApp(
                                        table,
                                        item.WhatsAppEnviado);
                                }
                            });
                        });

                    // Pie de página corporativo
                    page.Footer()
                        .Element(FooterEmpresa.Dibujar);
                });
            });

            return pdf.GeneratePdf();
        }

        private static void CeldaCabecera(
            TableCellDescriptor table,
            string texto)
        {
            table.Cell()
                .Background(PdfColors.VerdePrincipal)
                .Padding(5)
                .Text(texto)
                .FontSize(7)
                .Bold()
                .FontColor(PdfColors.Blanco);
        }

        private static void CeldaCuerpo(
            TableDescriptor table,
            string texto)
        {
            table.Cell()
                .BorderBottom(1)
                .BorderColor(PdfColors.GrisClaro)
                .Padding(5)
                .Text(texto)
                .FontSize(7);
        }

        private static void CeldaWhatsApp(
            TableDescriptor table,
            bool? enviado)
        {
            var texto = enviado switch
            {
                true => "Enviado",
                false => "Falló",
                _ => "Sin registro"
            };

            var color = enviado switch
            {
                true => PdfColors.VerdeOscuro,
                false => PdfColors.Rojo,
                _ => PdfColors.GrisMedio
            };

            table.Cell()
                .BorderBottom(1)
                .BorderColor(PdfColors.GrisClaro)
                .Padding(5)
                .Text(texto)
                .FontSize(7)
                .FontColor(color);
        }
    }
}
