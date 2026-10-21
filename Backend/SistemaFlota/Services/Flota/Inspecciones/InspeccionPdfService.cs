using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SistemaFlota.Services.Pdf.Components;
using SistemaFlota.Services.Pdf.Styles;

namespace SistemaFlota.Services.Flota.Inspecciones
{
    public class InspeccionPdfService : IInspeccionPdfService
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public InspeccionPdfService(
            AppDbContext context,
            IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        public async Task<byte[]> GenerarPdfAsync(int id)
        {
            var inspeccion = await _context.Inspecciones
                .AsNoTracking()
                .Include(i => i.Vehiculo)
                .Include(i => i.Conductor)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (inspeccion == null)
            {
                throw new KeyNotFoundException(
                    $"No existe la inspección con ID {id}.");
            }

            var empresa = await _context.ConfiguracionEmpresa
                .FirstOrDefaultAsync();

            if (empresa == null)
            {
                throw new Exception(
                    "No existe la configuración de la empresa.");
            }

            var detalles = await _context.InspeccionDetalles
                .AsNoTracking()
                .Where(d => d.InspeccionId == id)
                .Select(d => new
                {
                    d.ChecklistItemId,
                    NombreItem = d.ChecklistItem != null
                        ? d.ChecklistItem.Descripcion
                        : d.DescripcionItem,
                    d.Estado,
                    d.Observacion,
                    d.FotoEvidencia
                })
                .ToListAsync();

            var totalItems = detalles.Count;

            var totalNoConformes = detalles.Count(
                d => d.Estado == "No conforme");

            var porcentaje = totalItems == 0
                ? 0
                : Math.Round(
                    totalNoConformes * 100.0 / totalItems,
                    2);

            var estadoGeneral = totalItems == 0 ||
                                totalNoConformes == 0
                ? "APTO"
                : porcentaje <= 20
                    ? "APTO CON OBSERVACIONES"
                    : "NO APTO";

            var fecha = inspeccion.Fecha.ToString(
                "dd/MM/yyyy HH:mm");

            var nombreConductor =
                inspeccion.Conductor?.Nombre ?? "No registrado";

            var placa =
                inspeccion.Vehiculo?.Placa ?? "No registrada";

            var rutaLogo = ObtenerLogo();
            var rutaMarcaAgua = ObtenerMarcaAgua();

            var rutaOdometro = ObtenerRutaImagen(
                inspeccion.FotoOdometro);

            var rutaFirma = ObtenerRutaImagen(
                inspeccion.FirmaCondutor);

            var pdf = Document.Create(document =>
            {
                document.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(20);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    // Marca de agua corporativa
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

                            // Encabezado corporativo: solo aparece una vez
                        content.Item()
                            .Element(container =>
                            {
                                HeaderEmpresa.Dibujar(
                                    container,
                                    rutaLogo,
                                    empresa,
                                    "F-GC-027 V2",
                                    "",
                                    "REPORTE DE INSPECCIÓN",
                                    id.ToString());
                            });

                            // Estado general
                            content.Item()
                                .Border(1)
                                .BorderColor(
                                    ColorEstado(estadoGeneral))
                                .Background(PdfColors.Fondo)
                                .Padding(12)
                                .Column(col =>
                                {
                                    col.Item()
                                        .Text(estadoGeneral)
                                        .FontSize(15)
                                        .Bold()
                                        .FontColor(
                                            ColorEstado(estadoGeneral));

                                    col.Item()
                                        .Text(
                                            $"Ítems no conformes: " +
                                            $"{totalNoConformes}/{totalItems} " +
                                            $"({porcentaje:0.##}%)")
                                        .Style(PdfStyles.Label);
                                });

                            // Información general
                            content.Item().Element(card =>
                                card.Border(1)
                                    .BorderColor(PdfColors.GrisClaro)
                                    .CornerRadius(6)
                                    .Padding(12)
                                    .Column(col =>
                                    {
                                        col.Item()
                                            .Text("DATOS GENERALES")
                                            .Style(PdfStyles.Subtitulo);

                                        col.Item()
                                            .PaddingTop(8)
                                            .Row(row =>
                                            {
                                                row.RelativeItem()
                                                    .Column(c =>
                                                    {
                                                        Campo(
                                                            c,
                                                            "Fecha",
                                                            fecha);

                                                        Campo(
                                                            c,
                                                            "Conductor",
                                                            nombreConductor);

                                                        Campo(
                                                            c,
                                                            "Vehículo",
                                                            placa);
                                                    });

                                                row.RelativeItem()
                                                    .Column(c =>
                                                    {
                                                        Campo(
                                                            c,
                                                            "Kilometraje",
                                                            $"{inspeccion.Kilometraje:N0} km");

                                                        Campo(
                                                            c,
                                                            "ID inspección",
                                                            id.ToString());

                                                        Campo(
                                                            c,
                                                            "Total de ítems",
                                                            totalItems.ToString());
                                                    });
                                            });
                                    }));

                            // Tabla checklist
                            content.Item()
                                .Text("DETALLE DEL CHECKLIST")
                                .Style(PdfStyles.Subtitulo);

                            content.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.ConstantColumn(28);
                                    columns.RelativeColumn(3);
                                    columns.RelativeColumn(1.5f);
                                    columns.RelativeColumn(3);
                                });

                                table.Header(header =>
                                {
                                    CeldaCabecera(header, "#");
                                    CeldaCabecera(header, "Ítem");
                                    CeldaCabecera(header, "Estado");
                                    CeldaCabecera(
                                        header,
                                        "Observación");
                                });

                                for (var i = 0; i < detalles.Count; i++)
                                {
                                    var detalle = detalles[i];

                                    CeldaCuerpo(
                                        table,
                                        (i + 1).ToString());

                                    CeldaCuerpo(
                                        table,
                                        detalle.NombreItem
                                            ?? detalle.ChecklistItemId?.ToString()
                                            ?? "-");

                                    var estado =
                                        detalle.Estado ?? "-";

                                    table.Cell()
                                        .BorderBottom(1)
                                        .BorderColor(
                                            PdfColors.GrisClaro)
                                        .Padding(5)
                                        .Text(estado)
                                        .FontSize(8)
                                        .Bold()
                                        .FontColor(
                                            ColorEstadoItem(estado));

                                    CeldaCuerpo(
                                        table,
                                        detalle.Observacion ?? "-");
                                }
                            });

                            // Fotografía del odómetro
                            if (File.Exists(rutaOdometro))
                            {
                                content.Item().Column(col =>
                                {
                                    col.Item()
                                        .Text(
                                            "FOTOGRAFÍA DEL ODÓMETRO")
                                        .Style(PdfStyles.Subtitulo);

                                    col.Item()
                                        .MaxHeight(180)
                                        .AlignCenter()
                                        .Image(rutaOdometro)
                                        .FitArea();
                                });
                            }

                            // Firma del conductor
                            if (File.Exists(rutaFirma))
                            {
                                content.Item().Column(col =>
                                {
                                    col.Item()
                                        .Text("FIRMA DEL CONDUCTOR")
                                        .Style(PdfStyles.Subtitulo);

                                    col.Item()
                                        .Width(200)
                                        .Height(90)
                                        .Border(1)
                                        .BorderColor(
                                            PdfColors.GrisClaro)
                                        .Padding(5)
                                        .Image(rutaFirma)
                                        .FitArea();

                                    col.Item()
                                        .Text(nombreConductor)
                                        .Style(PdfStyles.Label);
                                });
                            }
                        });

                    // Pie de página corporativo
                    page.Footer()
                        .Element(FooterEmpresa.Dibujar);
                });
            });

            return pdf.GeneratePdf();
        }

        private string ObtenerLogo()
        {
            return Path.Combine(
                _environment.ContentRootPath,
                "wwwroot",
                "config",
                "logo.png");
        }

        private string ObtenerMarcaAgua()
        {
            return Path.Combine(
                _environment.ContentRootPath,
                "wwwroot",
                "config",
                "iguana3.png");
        }

        private string ObtenerRutaImagen(string? nombreArchivo)
        {
            if (string.IsNullOrWhiteSpace(nombreArchivo))
                return string.Empty;

            var nombre = Path.GetFileName(nombreArchivo);

            return Path.Combine(
                _environment.ContentRootPath,
                "wwwroot",
                "inspecciones",
                nombre);
        }

        private static void Campo(
            ColumnDescriptor columna,
            string etiqueta,
            string valor)
        {
            columna.Item()
                .PaddingBottom(6)
                .Column(c =>
                {
                    c.Item()
                        .Text(etiqueta)
                        .Style(PdfStyles.Label);

                    c.Item()
                        .Text(valor)
                        .Style(PdfStyles.Valor);
                });
        }

        private static void CeldaCabecera(
            TableCellDescriptor table,
            string texto)
        {
            table.Cell()
                .Background(PdfColors.VerdePrincipal)
                .Padding(6)
                .Text(texto)
                .FontSize(8)
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
                .FontSize(8);
        }

        private static string ColorEstado(string estado)
        {
            return estado switch
            {
                "APTO" => PdfColors.VerdeOscuro,
                "APTO CON OBSERVACIONES" => PdfColors.Amarillo,
                "NO APTO" => PdfColors.Rojo,
                _ => PdfColors.GrisMedio
            };
        }

        private static string ColorEstadoItem(string estado)
        {
            return estado switch
            {
                "Conforme" => PdfColors.VerdeOscuro,
                "No conforme" => PdfColors.Rojo,
                "No aplica" => PdfColors.GrisMedio,
                _ => PdfColors.AzulOscuro
            };
        }
    }
}