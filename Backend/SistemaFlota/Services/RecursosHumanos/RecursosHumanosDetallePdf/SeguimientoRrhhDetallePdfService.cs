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

                    // TÍTULO Y FOLIO

                    col.Item()
                        .PaddingTop(12)
                        .Row(row =>
                        {
                            row.RelativeItem().Column(c =>
                            {
                                c.Item()
                                    .Text("FICHA INDIVIDUAL DE SEGUIMIENTO")
                                    .Bold()
                                    .FontSize(13)
                                    .FontColor(PdfColors.VerdeOscuro);

                                c.Item()
                                    .PaddingTop(2)
                                    .Text($"Registro N.º {registro.Id}  |  Estado: {registro.Estado ?? "-"}  |  Prioridad: {registro.Prioridad ?? "-"}")
                                    .FontSize(8.5f)
                                    .FontColor(PdfColors.GrisOscuro);
                            });
                        });

                    // 1. INFORMACIÓN GENERAL Y PROGRAMACIÓN

                    col.Item()
                        .PaddingTop(12)
                        .PreventPageBreak()
                        .Element(container =>
                        {
                            Card.Dibujar(
                                container,
                                "1. INFORMACIÓN GENERAL Y PROGRAMACIÓN",
                                contenido =>
                                {
                                    contenido.Item().Row(row =>
                                    {
                                        row.RelativeItem(1.4f).Element(c =>
                                            CampoDetalle(c, "Área", registro.Area));

                                        row.ConstantItem(10);

                                        row.RelativeItem(1.1f).Element(c =>
                                            CampoDetalle(c, "Periodo", $"{NombreMes(registro.Mes)} {registro.Anio}"));

                                        row.ConstantItem(10);

                                        row.RelativeItem(1.3f).Element(c =>
                                            CampoDetalle(c, "Fuente", registro.Fuente));
                                    });

                                    contenido.Item()
                                        .PaddingTop(8)
                                        .Row(row =>
                                        {
                                            row.RelativeItem(1.4f).Element(c =>
                                                CampoDetalle(c, "Responsable", registro.Responsable));

                                            row.ConstantItem(10);

                                            row.RelativeItem(1f).Element(c =>
                                                CampoDetalle(c, "Fecha de ejecución", registro.FechaEjecucion?.ToString("dd/MM/yyyy")));

                                            row.ConstantItem(10);

                                            row.RelativeItem(1f).Element(c =>
                                                CampoDetalle(c, "Fecha de seguimiento", registro.FechaSeguimiento?.ToString("dd/MM/yyyy")));
                                        });
                                }
                            );
                        });

                    // 2. DESCRIPCIÓN, RIESGO Y PLAN DE ACCIÓN

                    col.Item()
                        .PaddingTop(10)
                        .PreventPageBreak()
                        .Element(container =>
                        {
                            Card.Dibujar(
                                container,
                                "2. DESCRIPCIÓN, RIESGO Y PLAN DE ACCIÓN",
                                contenido =>
                                {
                                    contenido.Item().Element(c =>
                                        CampoDetalle(c, "Descripción del hallazgo / seguimiento", registro.Descripcion));

                                    contenido.Item()
                                        .PaddingTop(8)
                                        .Row(row =>
                                        {
                                            row.RelativeItem(1f).Element(c =>
                                                CampoDetalle(c, "Factor de riesgo", registro.FactorRiesgo));

                                            row.ConstantItem(10);

                                            row.RelativeItem(2f).Element(c =>
                                                CampoDetalle(c, "Plan de acción sugerido", registro.PlanAccionSugerido));
                                        });
                                }
                            );
                        });

                    // 3. OBSERVACIONES Y AUDITORÍA

                    col.Item()
                        .PaddingTop(10)
                        .PreventPageBreak()
                        .Element(container =>
                        {
                            Card.Dibujar(
                                container,
                                "3. OBSERVACIONES Y REGISTRO",
                                contenido =>
                                {
                                    contenido.Item().Element(c =>
                                        CampoDetalle(c, "Observaciones", registro.Observaciones));

                                    contenido.Item()
                                        .PaddingTop(8)
                                        .Row(row =>
                                        {
                                            row.RelativeItem().Element(c =>
                                                CampoDetalle(c, "Creado por", registro.CreadoPor.ToString()));

                                            row.ConstantItem(10);

                                            row.RelativeItem().Element(c =>
                                                CampoDetalle(c, "Fecha de creación", registro.FechaCreacion.ToString("dd/MM/yyyy HH:mm")));

                                            row.ConstantItem(10);

                                            row.RelativeItem().Element(c =>
                                                CampoDetalle(c, "Modificado por", registro.ModificadoPor?.ToString()));

                                            row.ConstantItem(10);

                                            row.RelativeItem().Element(c =>
                                                CampoDetalle(c, "Fecha de modificación", registro.FechaModificacion?.ToString("dd/MM/yyyy HH:mm")));
                                        });
                                }
                            );
                        });

                    // 4. EVIDENCIAS FOTOGRÁFICAS

                    col.Item()
                        .PaddingTop(10)
                        .Element(container =>
                        {
                            Card.Dibujar(
                                container,
                                "4. EVIDENCIAS FOTOGRÁFICAS",
                                contenido =>
                                {
                                    var fotos = registro.Fotos?.ToList() ?? new();

                                    if (!fotos.Any())
                                    {
                                        contenido.Item()
                                            .Text("No hay fotografías registradas.")
                                            .FontSize(8.5f)
                                            .FontColor(PdfColors.GrisOscuro);
                                        return;
                                    }

                                    var evidencias = fotos
                                        .Where(f => string.Equals(f.TipoFoto, "evidencia", StringComparison.OrdinalIgnoreCase))
                                        .Select(f => new { f.NombreArchivo, Ruta = ObtenerRutaFoto(f.NombreArchivo) })
                                        .Where(f => !string.IsNullOrEmpty(f.Ruta) && File.Exists(f.Ruta))
                                        .ToList();

                                    var seguimientos = fotos
                                        .Where(f => string.Equals(f.TipoFoto, "seguimiento", StringComparison.OrdinalIgnoreCase))
                                        .Select(f => new { f.NombreArchivo, Ruta = ObtenerRutaFoto(f.NombreArchivo) })
                                        .Where(f => !string.IsNullOrEmpty(f.Ruta) && File.Exists(f.Ruta))
                                        .ToList();

                                    if (!evidencias.Any() && !seguimientos.Any())
                                    {
                                        contenido.Item()
                                            .Text("No se encontraron los archivos fotográficos en el servidor.")
                                            .FontSize(8.5f)
                                            .FontColor(PdfColors.GrisOscuro);
                                        return;
                                    }

                                    if (evidencias.Any())
                                    {
                                        contenido.Item()
                                            .Text("EVIDENCIAS INICIALES")
                                            .Bold()
                                            .FontSize(9)
                                            .FontColor(PdfColors.VerdeOscuro);

                                        contenido.Item()
                                            .PaddingTop(4)
                                            .Element(c => RenderizarGrillaFotos(c, evidencias.Select(x => x.Ruta).ToList()));
                                    }

                                    if (seguimientos.Any())
                                    {
                                        contenido.Item()
                                            .PaddingTop(evidencias.Any() ? 8 : 0)
                                            .Text("FOTOGRAFÍAS DE SEGUIMIENTO")
                                            .Bold()
                                            .FontSize(9)
                                            .FontColor(PdfColors.VerdeOscuro);

                                        contenido.Item()
                                            .PaddingTop(4)
                                            .Element(c => RenderizarGrillaFotos(c, seguimientos.Select(x => x.Ruta).ToList()));
                                    }
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

        private void RenderizarGrillaFotos(IContainer container, List<string> rutas)
        {
            container.Column(col =>
            {
                for (int i = 0; i < rutas.Count; i += 2)
                {
                    var primera = rutas[i];
                    var segunda = i + 1 < rutas.Count ? rutas[i + 1] : null;

                    col.Item()
                        .PaddingBottom(4)
                        .Row(row =>
                        {
                            row.RelativeItem()
                                .Element(c => TarjetaFoto(c, primera));

                            row.ConstantItem(4);

                            if (segunda != null)
                            {
                                row.RelativeItem()
                                    .Element(c => TarjetaFoto(c, segunda));
                            }
                            else
                            {
                                row.RelativeItem();
                            }
                        });
                }
            });
        }

        private void TarjetaFoto(IContainer container, string ruta)
        {
            container
                .Border(0.5f)
                .BorderColor(PdfColors.GrisClaro)
                .Background(Colors.White)
                .Padding(2)
                .AlignCenter()
                .AlignMiddle()
                .MaxHeight(125)
                .Image(ruta)
                .FitArea();
        }

        private void CampoDetalle(
            IContainer container,
            string titulo,
            string? valor)
        {
            container.Column(col =>
            {
                col.Item()
                    .Text(titulo)
                    .Bold()
                    .FontSize(8)
                    .FontColor(PdfColors.VerdeOscuro);

                col.Item()
                    .PaddingTop(2)
                    .Text(
                        string.IsNullOrWhiteSpace(valor)
                            ? "No registrado"
                            : valor.Trim()
                    )
                    .FontSize(8.8f)
                    .FontColor(PdfColors.GrisOscuro);
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

        private string NombreMes(int mes)
        {
            return mes >= 1 && mes <= 12
                ? new CultureInfo("es-CO")
                    .DateTimeFormat
                    .GetMonthName(mes)
                    .ToUpperInvariant()
                : mes.ToString();
        }
    }
}
