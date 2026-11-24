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

                    // 1. INFORMACIÓN GENERAL Y PROGRAMACIÓN

                    col.Item()
                        .PaddingTop(12)
                        .PreventPageBreak()
                        .Element(container =>
                        {
                            Card.Dibujar(
                                container,
                                $"SEGUIMIENTO #{registro.Id} — INFORMACIÓN GENERAL",
                                contenido =>
                                {
                                    contenido.Item().Row(row =>
                                    {
                                        row.RelativeItem().Element(c =>
                                            CampoIndividual(c, "ÁREA", registro.Area));

                                        row.ConstantItem(12);

                                        row.RelativeItem().Element(c =>
                                            CampoIndividual(c, "PERÍODO", $"{NombreMesCapitalizado(registro.Mes)} {registro.Anio}"));

                                        row.ConstantItem(12);

                                        row.RelativeItem().Element(c =>
                                            CampoIndividual(c, "FUENTE", registro.Fuente));
                                    });

                                    contenido.Item()
                                        .PaddingTop(10)
                                        .Row(row =>
                                        {
                                            row.RelativeItem().Element(c =>
                                                CampoIndividual(c, "FACTOR DE RIESGO", registro.FactorRiesgo));

                                            row.ConstantItem(12);

                                            row.RelativeItem().Element(c =>
                                                CampoIndividual(c, "RESPONSABLE", registro.Responsable));

                                            row.ConstantItem(12);

                                            row.RelativeItem().Element(c =>
                                                CampoEstadoPrioridad(c, registro.Estado, registro.Prioridad));
                                        });

                                    contenido.Item()
                                        .PaddingTop(10)
                                        .Row(row =>
                                        {
                                            row.RelativeItem().Element(c =>
                                                CampoIndividual(c, "FECHA EJECUCIÓN", registro.FechaEjecucion?.ToString("dd/MM/yyyy")));

                                            row.ConstantItem(12);

                                            row.RelativeItem().Element(c =>
                                                CampoIndividual(c, "FECHA SEGUIMIENTO", registro.FechaSeguimiento?.ToString("dd/MM/yyyy")));

                                            row.ConstantItem(12);

                                            row.RelativeItem();
                                        });
                                }
                            );
                        });

                    // 2. DESCRIPCIÓN Y PLAN DE ACCIÓN

                    col.Item()
                        .PaddingTop(10)
                        .PreventPageBreak()
                        .Element(container =>
                        {
                            Card.Dibujar(
                                container,
                                "DESCRIPCIÓN Y PLAN DE ACCIÓN",
                                contenido =>
                                {
                                    contenido.Item().Element(c =>
                                        CampoIndividual(c, "DESCRIPCIÓN", registro.Descripcion));

                                    contenido.Item()
                                        .PaddingTop(10)
                                        .Element(c =>
                                            CampoIndividual(c, "PLAN DE ACCIÓN", registro.PlanAccionSugerido));
                                }
                            );
                        });

                    // EVIDENCIA FOTOGRÁFICA INICIAL

                    var fotosOrdenadas = registro.Fotos?
                        .OrderBy(f => f.Id)
                        .ToList() ?? new();

                    var evidencias = fotosOrdenadas
                        .Where(f => string.Equals(f.TipoFoto, "evidencia", StringComparison.OrdinalIgnoreCase))
                        .Select(f => ObtenerRutaFoto(f.NombreArchivo))
                        .Where(r => !string.IsNullOrEmpty(r) && File.Exists(r))
                        .ToList();

                    if (evidencias.Any())
                    {
                        col.Item()
                            .PaddingTop(10)
                            .PreventPageBreak()
                            .Element(container =>
                            {
                                Card.Dibujar(
                                    container,
                                    $"EVIDENCIA FOTOGRÁFICA ({evidencias.Count} FOTOS)",
                                    contenido =>
                                    {
                                        contenido.Item().Element(c =>
                                            FilaMiniaturas(c, evidencias));
                                    }
                                );
                            });
                    }

                    // REGISTRO FOTOGRÁFICO POSTERIOR

                    var seguimientos = fotosOrdenadas
                        .Where(f => string.Equals(f.TipoFoto, "seguimiento", StringComparison.OrdinalIgnoreCase))
                        .Select(f => ObtenerRutaFoto(f.NombreArchivo))
                        .Where(r => !string.IsNullOrEmpty(r) && File.Exists(r))
                        .ToList();

                    if (seguimientos.Any())
                    {
                        col.Item()
                            .PaddingTop(10)
                            .PreventPageBreak()
                            .Element(container =>
                            {
                                Card.Dibujar(
                                    container,
                                    $"REGISTRO FOTOGRÁFICO POSTERIOR ({seguimientos.Count} FOTOS)",
                                    contenido =>
                                    {
                                        contenido.Item().Element(c =>
                                            FilaMiniaturas(c, seguimientos));
                                    }
                                );
                            });
                    }

                    // 3. OBSERVACIONES Y REGISTRO

                    col.Item()
                        .PaddingTop(10)
                        .PreventPageBreak()
                        .Element(container =>
                        {
                            Card.Dibujar(
                                container,
                                "OBSERVACIONES Y REGISTRO",
                                contenido =>
                                {
                                    contenido.Item().Element(c =>
                                        CampoIndividual(c, "OBSERVACIONES", registro.Observaciones));

                                    contenido.Item()
                                        .PaddingTop(10)
                                        .Row(row =>
                                        {
                                            row.RelativeItem().Element(c =>
                                                CampoIndividual(c, "CREADO POR", $"Usuario #{registro.CreadoPor}"));

                                            row.ConstantItem(12);

                                            row.RelativeItem().Element(c =>
                                                CampoIndividual(c, "FECHA CREACIÓN", registro.FechaCreacion.ToString("dd/MM/yyyy HH:mm")));

                                            row.ConstantItem(12);

                                            row.RelativeItem().Element(c =>
                                                CampoIndividual(
                                                    c,
                                                    "ÚLTIMA MODIFICACIÓN",
                                                    registro.FechaModificacion?.ToString("dd/MM/yyyy HH:mm")
                                                ));
                                        });
                                }
                            );
                        });
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

        private void CampoIndividual(IContainer container, string etiqueta, string? valor)
        {
            container.Column(col =>
            {
                col.Item()
                    .Text(etiqueta)
                    .Bold()
                    .FontSize(8)
                    .FontColor(PdfColors.VerdeOscuro);

                col.Item()
                    .PaddingTop(2)
                    .Text(string.IsNullOrWhiteSpace(valor) ? "No registrado" : valor.Trim())
                    .FontSize(9)
                    .FontColor(PdfColors.GrisOscuro);
            });
        }

        private void CampoEstadoPrioridad(IContainer container, string? estado, string? prioridad)
        {
            container.Column(col =>
            {
                col.Item()
                    .Text("ESTADO / PRIORIDAD")
                    .Bold()
                    .FontSize(8)
                    .FontColor(PdfColors.VerdeOscuro);

                col.Item()
                    .PaddingTop(2)
                    .Text(text =>
                    {
                        var textoEstado = string.IsNullOrWhiteSpace(estado) ? "No registrado" : estado.Trim();
                        text.Span(textoEstado)
                            .Bold()
                            .FontSize(9)
                            .FontColor(ObtenerColorEstado(estado));

                        text.Span("  |  ")
                            .FontSize(9)
                            .FontColor(Colors.Grey.Darken1);

                        var textoPrioridad = string.IsNullOrWhiteSpace(prioridad) ? "No registrada" : prioridad.Trim();
                        text.Span(textoPrioridad)
                            .Bold()
                            .FontSize(9)
                            .FontColor(ObtenerColorPrioridad(prioridad));
                    });
            });
        }

        private string ObtenerColorPrioridad(string? prioridad)
        {
            return prioridad switch
            {
                "Alta" => Colors.Red.Darken1,
                "Media" => Colors.Orange.Darken2,
                "Baja" => Colors.Grey.Darken1,
                _ => PdfColors.GrisOscuro
            };
        }

        private string ObtenerColorEstado(string? estado)
        {
            return estado switch
            {
                "Ejecutada" => Colors.Green.Darken2,
                "En proceso" => Colors.Blue.Darken1,
                "Abierta" => Colors.Orange.Darken2,
                _ => PdfColors.GrisOscuro
            };
        }

        private void FilaMiniaturas(IContainer container, List<string> rutas)
        {
            const int porFila = 6;

            container.Column(col =>
            {
                for (int i = 0; i < rutas.Count; i += porFila)
                {
                    var fila = col.Item().PreventPageBreak();

                    if (i > 0)
                        fila = fila.PaddingTop(5);

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
                                    .BorderColor(PdfColors.GrisClaro)
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

        private string ObtenerWebRoot()
        {
            return _environment.WebRootPath
                ?? Path.Combine(
                    _environment.ContentRootPath,
                    "wwwroot"
                );
        }

        private string ObtenerLogo(string? logo)
        {
            return Path.Combine(
                ObtenerWebRoot(),
                "config",
                "logo.png"
            );
        }

        private string ObtenerMarcaAgua()
        {
            return Path.Combine(
                ObtenerWebRoot(),
                "config",
                "iguana3.png"
            );
        }

        private string ObtenerRutaFoto(string? nombreArchivo)
        {
            if (string.IsNullOrWhiteSpace(nombreArchivo))
                return string.Empty;

            return Path.Combine(
                ObtenerWebRoot(),
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
