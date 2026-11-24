using System.Globalization;
using System.IO;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SistemaFlota.Models;
using SistemaFlota.Services.Pdf.Components;
using SistemaFlota.Services.Pdf.Styles;

namespace SistemaFlota.Services.RecursosHumanos.RecursosHumanosDetallePdf
{
    public class SeguimientoRrhhDetallePdfService
        : ISeguimientoRrhhDetallePdfService
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public SeguimientoRrhhDetallePdfService(
            AppDbContext context,
            IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        public async Task<byte[]> GenerarPdfAsync(int id)
        {
            var registro = await _context.SeguimientosRrhh
                .AsNoTracking()
                .Include(s => s.Fotos)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (registro == null)
                throw new KeyNotFoundException(
                    "No se encontró el seguimiento solicitado."
                );

            var empresa = await _context.ConfiguracionEmpresa
                .AsNoTracking()
                .FirstOrDefaultAsync();

            if (empresa == null)
                throw new Exception(
                    "No existe la configuración de la empresa."
                );

            var pdf = Document.Create(document =>
            {
                document.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(22);
                    page.DefaultTextStyle(x => x.FontSize(9));

                    ConstruirDocumento(page, registro, empresa);
                });
            });

            return pdf.GeneratePdf();
        }

        private void ConstruirDocumento(
            PageDescriptor page,
            SeguimientoRrhh registro,
            ConfiguracionEmpresa empresa)
        {
            // MARCA DE AGUA

            page.Background()
                .Element(container =>
                    MarcaAgua.Dibujar(
                        container,
                        ObtenerMarcaAgua()
                    )
                );

            // CONTENIDO

            page.Content()
                .PaddingTop(8)
                .Column(col =>
                {
                    // ENCABEZADO EMPRESA

                    col.Item().Element(container =>
                    {
                        HeaderEmpresa.Dibujar(
                            container,
                            ObtenerLogo(empresa.Logo),
                            empresa,
                            "SEGUIMIENTO DE MATRIZ DE MEJORA",
                            "F-SST-74 V2",
                            "15-09-25",
                            ""
                        );
                    });

                    // TÍTULO ESTILO MODAL

                    col.Item()
                        .PaddingTop(12)
                        .Row(row =>
                        {
                            row.RelativeItem().Text(text =>
                            {
                                text.Span($"Seguimiento #{registro.Id}")
                                    .Bold()
                                    .FontSize(14)
                                    .FontColor(Colors.Grey.Darken4);

                                text.Span($" — {registro.Area ?? "Sin área"}")
                                    .Bold()
                                    .FontSize(14)
                                    .FontColor(Colors.Grey.Darken2);
                            });
                        });

                    // ETIQUETAS ESTADO Y PRIORIDAD

                    col.Item()
                        .PaddingTop(6)
                        .Row(row =>
                        {
                            if (!string.IsNullOrWhiteSpace(registro.Estado))
                            {
                                row.AutoItem()
                                    .Background(Colors.Green.Lighten4)
                                    .PaddingVertical(3)
                                    .PaddingHorizontal(8)
                                    .Text(registro.Estado)
                                    .FontSize(7.5f)
                                    .Bold()
                                    .FontColor(Colors.Green.Darken3);

                                row.ConstantItem(6);
                            }

                            if (!string.IsNullOrWhiteSpace(registro.Prioridad))
                            {
                                row.AutoItem()
                                    .Background(Colors.Red.Lighten4)
                                    .PaddingVertical(3)
                                    .PaddingHorizontal(8)
                                    .Text(registro.Prioridad)
                                    .FontSize(7.5f)
                                    .Bold()
                                    .FontColor(Colors.Red.Darken2);
                            }
                        });

                    // ÁREA / PERÍODO / FUENTE / ÁREAS

                    col.Item()
                        .PaddingTop(10)
                        .Row(row =>
                        {
                            row.RelativeItem().Element(c =>
                                CampoSuperior(c, "ÁREA", registro.Area));

                            row.ConstantItem(12);

                            row.RelativeItem().Element(c =>
                                CampoSuperior(c, "PERÍODO", $"{NombreMesCapitalizado(registro.Mes)} {registro.Anio}"));
                        });

                    col.Item()
                        .PaddingTop(8)
                        .Row(row =>
                        {
                            row.RelativeItem().Element(c =>
                                CampoSuperior(c, "FUENTE", registro.Fuente));

                            row.ConstantItem(12);

                            row.RelativeItem().Element(c =>
                                CampoSuperior(c, "ÁREAS", registro.Responsable));
                        });

                    // DESCRIPCIÓN

                    col.Item()
                        .PaddingTop(10)
                        .Element(c =>
                            BloqueTexto(c, "DESCRIPCIÓN", registro.Descripcion));

                    // EVIDENCIA FOTOGRÁFICA INICIAL

                    var fotos = registro.Fotos?.ToList() ?? new();

                    var evidencias = fotos
                        .Where(f => string.Equals(f.TipoFoto, "evidencia", StringComparison.OrdinalIgnoreCase))
                        .Select(f => ObtenerRutaFoto(f.NombreArchivo))
                        .Where(r => !string.IsNullOrEmpty(r) && File.Exists(r))
                        .ToList();

                    if (evidencias.Any())
                    {
                        col.Item()
                            .PaddingTop(10)
                            .PreventPageBreak()
                            .Element(c =>
                                SeccionMiniaturas(c, $"EVIDENCIA FOTOGRÁFICA ({evidencias.Count} FOTOS)", evidencias));
                    }

                    // FACTOR DE RIESGO / RESPONSABLE

                    col.Item()
                        .PaddingTop(10)
                        .Row(row =>
                        {
                            row.RelativeItem().Element(c =>
                                CampoSuperior(c, "FACTOR DE RIESGO", registro.FactorRiesgo));

                            row.ConstantItem(12);

                            row.RelativeItem().Element(c =>
                                CampoSuperior(c, "RESPONSABLE", registro.Responsable));
                        });

                    // FECHA EJECUCIÓN / FECHA SEGUIMIENTO

                    col.Item()
                        .PaddingTop(8)
                        .Row(row =>
                        {
                            row.RelativeItem().Element(c =>
                                CampoSuperior(c, "FECHA EJECUCIÓN", registro.FechaEjecucion?.ToString("dd/MM/yyyy")));

                            row.ConstantItem(12);

                            row.RelativeItem().Element(c =>
                                CampoSuperior(c, "FECHA SEGUIMIENTO", registro.FechaSeguimiento?.ToString("dd/MM/yyyy")));
                        });

                    // PLAN DE ACCIÓN

                    col.Item()
                        .PaddingTop(10)
                        .Element(c =>
                            BloqueTexto(c, "PLAN DE ACCIÓN", registro.PlanAccionSugerido));

                    // REGISTRO FOTOGRÁFICO POSTERIOR

                    var seguimientos = fotos
                        .Where(f => string.Equals(f.TipoFoto, "seguimiento", StringComparison.OrdinalIgnoreCase))
                        .Select(f => ObtenerRutaFoto(f.NombreArchivo))
                        .Where(r => !string.IsNullOrEmpty(r) && File.Exists(r))
                        .ToList();

                    if (seguimientos.Any())
                    {
                        col.Item()
                            .PaddingTop(10)
                            .PreventPageBreak()
                            .Element(c =>
                                SeccionMiniaturas(c, $"REGISTRO FOTOGRÁFICO POSTERIOR ({seguimientos.Count} FOTOS)", seguimientos));
                    }

                    // OBSERVACIONES

                    col.Item()
                        .PaddingTop(10)
                        .Element(c =>
                            BloqueTexto(c, "OBSERVACIONES", registro.Observaciones));

                    // CREADO POR

                    col.Item()
                        .PaddingTop(10)
                        .Element(c =>
                            CampoSuperior(
                                c,
                                "CREADO POR",
                                $"Usuario #{registro.CreadoPor} — {registro.FechaCreacion:dd/MM/yyyy HH:mm}"
                            ));
                });

            // PIE DE PÁGINA

            page.Footer()
                .Element(container =>
                    FooterEmpresa.Dibujar(
                        container,
                        "Layas Software - SST"
                    )
                );
        }

        private void CampoSuperior(IContainer container, string titulo, string? valor)
        {
            container.Column(col =>
            {
                col.Item()
                    .Text(titulo)
                    .Bold()
                    .FontSize(8.5f)
                    .FontColor(Colors.Grey.Darken4);

                col.Item()
                    .PaddingTop(2)
                    .Text(string.IsNullOrWhiteSpace(valor) ? "No registrado" : valor.Trim())
                    .Bold()
                    .FontSize(9.5f)
                    .FontColor(Colors.Grey.Darken1);
            });
        }

        private void BloqueTexto(IContainer container, string titulo, string? texto)
        {
            container.Column(col =>
            {
                col.Item()
                    .Text(titulo)
                    .Bold()
                    .FontSize(8.5f)
                    .FontColor(Colors.Grey.Darken4);

                col.Item()
                    .PaddingTop(4)
                    .Border(0.75f)
                    .BorderColor(Colors.Grey.Lighten1)
                    .Background(Colors.White)
                    .Padding(8)
                    .Text(string.IsNullOrWhiteSpace(texto) ? "No registrado" : texto.Trim())
                    .FontSize(9f)
                    .FontColor(Colors.Grey.Darken3);
            });
        }

        private void SeccionMiniaturas(IContainer container, string titulo, List<string> rutas)
        {
            const int porFila = 6;

            container.Column(col =>
            {
                col.Item()
                    .Text(titulo)
                    .Bold()
                    .FontSize(8.5f)
                    .FontColor(Colors.Grey.Darken4);

                for (int i = 0; i < rutas.Count; i += porFila)
                {
                    var fila = col.Item()
                        .PaddingTop(5)
                        .PreventPageBreak();

                    fila.Row(row =>
                    {
                        for (int c = 0; c < porFila; c++)
                        {
                            if (c > 0)
                                row.ConstantItem(6);

                            int idx = i + c;
                            if (idx < rutas.Count)
                            {
                                row.ConstantItem(72)
                                    .Height(72)
                                    .Border(0.75f)
                                    .BorderColor(Colors.Grey.Lighten1)
                                    .Padding(2)
                                    .Image(rutas[idx])
                                    .FitArea();
                            }
                            else
                            {
                                row.ConstantItem(72);
                            }
                        }
                    });
                }
            });
        }

        private string ObtenerLogo(string? logo)
        {
            return Path.Combine(
                _environment.ContentRootPath,
                "wwwroot",
                "config",
                "logo.png"
            );
        }

        private string ObtenerMarcaAgua()
        {
            return Path.Combine(
                _environment.ContentRootPath,
                "wwwroot",
                "config",
                "iguana3.png"
            );
        }

        private string ObtenerRutaFoto(string? nombreArchivo)
        {
            if (string.IsNullOrWhiteSpace(nombreArchivo))
                return string.Empty;

            var webRoot = _environment.WebRootPath
                ?? Path.Combine(
                    _environment.ContentRootPath,
                    "wwwroot"
                );

            return Path.Combine(
                webRoot,
                "seguimientos-rrhh",
                Path.GetFileName(nombreArchivo)
            );
        }

        private string NombreMesCapitalizado(int mes)
        {
            if (mes < 1 || mes > 12)
                return mes.ToString();

            var nombre = new CultureInfo("es-CO")
                .DateTimeFormat
                .GetMonthName(mes);

            return char.ToUpper(nombre[0]) + nombre[1..];
        }
    }
}
