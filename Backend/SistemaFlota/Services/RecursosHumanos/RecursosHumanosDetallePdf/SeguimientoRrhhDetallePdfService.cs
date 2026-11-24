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

                    // CARD: SEGUIMIENTO # — ÁREA + BADGES + ÁREA / PERÍODO / FUENTE

                    col.Item()
                        .PaddingTop(12)
                        .PreventPageBreak()
                        .Element(container =>
                        {
                            Card.Dibujar(
                                container,
                                $"SEGUIMIENTO #{registro.Id} — {(registro.Area ?? "SIN ÁREA").ToUpperInvariant()}",
                                contenido =>
                                {
                                    contenido.Item().Row(row =>
                                    {
                                        if (!string.IsNullOrWhiteSpace(registro.Estado))
                                        {
                                            var (fondoEstado, textoEstado) = ObtenerColoresBadgeEstado(registro.Estado);

                                            row.AutoItem()
                                                .Background(fondoEstado)
                                                .PaddingVertical(3)
                                                .PaddingHorizontal(8)
                                                .Text(registro.Estado)
                                                .FontSize(7.5f)
                                                .Bold()
                                                .FontColor(textoEstado);

                                            row.ConstantItem(6);
                                        }

                                        if (!string.IsNullOrWhiteSpace(registro.Prioridad))
                                        {
                                            var (fondoPrioridad, textoPrioridad) = ObtenerColoresBadgePrioridad(registro.Prioridad);

                                            row.AutoItem()
                                                .Background(fondoPrioridad)
                                                .PaddingVertical(3)
                                                .PaddingHorizontal(8)
                                                .Text(registro.Prioridad)
                                                .FontSize(7.5f)
                                                .Bold()
                                                .FontColor(textoPrioridad);
                                        }
                                    });

                                    contenido.Item()
                                        .PaddingTop(10)
                                        .Row(row =>
                                        {
                                            row.RelativeItem().Element(c =>
                                                CampoSuperior(c, "ÁREA", registro.Area));

                                            row.ConstantItem(12);

                                            row.RelativeItem().Element(c =>
                                                CampoSuperior(c, "PERÍODO", $"{NombreMesCapitalizado(registro.Mes)} {registro.Anio}"));

                                            row.ConstantItem(12);

                                            row.RelativeItem().Element(c =>
                                                CampoSuperior(c, "FUENTE", registro.Fuente));
                                        });
                                }
                            );
                        });

                    // DESCRIPCIÓN

                    col.Item()
                        .PaddingTop(10)
                        .Element(c =>
                            BloqueTexto(c, "DESCRIPCIÓN", registro.Descripcion));

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
                            .Element(c =>
                                SeccionMiniaturas(c, $"EVIDENCIA FOTOGRÁFICA ({evidencias.Count} FOTOS)", evidencias));
                    }

                    // CARD: RIESGO, RESPONSABLE Y PROGRAMACIÓN

                    col.Item()
                        .PaddingTop(10)
                        .PreventPageBreak()
                        .Element(container =>
                        {
                            Card.Dibujar(
                                container,
                                "RIESGO, RESPONSABLE Y PROGRAMACIÓN",
                                contenido =>
                                {
                                    contenido.Item().Row(row =>
                                    {
                                        row.RelativeItem().Element(c =>
                                            CampoSuperior(c, "FACTOR DE RIESGO", registro.FactorRiesgo));

                                        row.ConstantItem(12);

                                        row.RelativeItem().Element(c =>
                                            CampoSuperior(c, "RESPONSABLE", registro.Responsable));
                                    });

                                    contenido.Item()
                                        .PaddingTop(8)
                                        .Row(row =>
                                        {
                                            row.RelativeItem().Element(c =>
                                                CampoSuperior(c, "FECHA EJECUCIÓN", registro.FechaEjecucion?.ToString("dd/MM/yyyy")));

                                            row.ConstantItem(12);

                                            row.RelativeItem().Element(c =>
                                                CampoSuperior(c, "FECHA SEGUIMIENTO", registro.FechaSeguimiento?.ToString("dd/MM/yyyy")));
                                        });
                                }
                            );
                        });

                    // PLAN DE ACCIÓN

                    col.Item()
                        .PaddingTop(10)
                        .Element(c =>
                            BloqueTexto(c, "PLAN DE ACCIÓN", registro.PlanAccionSugerido));

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

        private (string Fondo, string Texto) ObtenerColoresBadgeEstado(string estado)
        {
            return estado switch
            {
                "Ejecutada" => (Colors.Green.Lighten4, Colors.Green.Darken3),
                "En proceso" => (Colors.Blue.Lighten4, Colors.Blue.Darken3),
                "Abierta" => (Colors.Orange.Lighten4, Colors.Orange.Darken3),
                _ => (Colors.Grey.Lighten3, Colors.Grey.Darken3)
            };
        }

        private (string Fondo, string Texto) ObtenerColoresBadgePrioridad(string prioridad)
        {
            return prioridad switch
            {
                "Alta" => (Colors.Red.Lighten4, Colors.Red.Darken2),
                "Media" => (Colors.Orange.Lighten4, Colors.Orange.Darken2),
                "Baja" => (Colors.Grey.Lighten3, Colors.Grey.Darken2),
                _ => (Colors.Grey.Lighten3, Colors.Grey.Darken3)
            };
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
