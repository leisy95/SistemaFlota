
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SistemaFlota.Models;
using SistemaFlota.Services.Pdf.Components;
using SistemaFlota.Services.Pdf.Styles;
using System.Globalization;
using System.IO;

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
            // CONSULTAR SEGUIMIENTO

            var registro = await _context.SeguimientosRrhh
                .AsNoTracking()
                .Include(s => s.Fotos)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (registro == null)
                throw new KeyNotFoundException(
                    "No se encontró el seguimiento solicitado."
                );

            // CONFIGURACIÓN EMPRESA

            var empresa = await _context.ConfiguracionEmpresa
                .AsNoTracking()
                .FirstOrDefaultAsync();

            if (empresa == null)
                throw new Exception(
                    "No existe la configuración de la empresa."
                );

            // GENERAR PDF

            var pdf = Document.Create(document =>
            {
                document.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(25);
                    page.DefaultTextStyle(x => x.FontSize(9));

                    ConstruirDocumento(page, registro, empresa);
                });
            });

            return pdf.GeneratePdf();
        }

        // CONSTRUIR DOCUMENTO

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
                .PaddingTop(10)
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

                    // TÍTULO

                    col.Item()
                        .PaddingTop(15)
                        .AlignCenter()
                        .Text("FICHA INDIVIDUAL DE SEGUIMIENTO")
                        .Bold()
                        .FontSize(14)
                        .FontColor(PdfColors.VerdeOscuro);

                    col.Item()
                        .PaddingTop(5)
                        .AlignCenter()
                        .Text($"REGISTRO N.º {registro.Id}")
                        .FontSize(10)
                        .FontColor(PdfColors.GrisOscuro);

                    // 1. INFORMACIÓN GENERAL

                    col.Item()
                        .PaddingTop(15)
                        .Element(container =>
                        {
                            Card.Dibujar(
                                container,
                                "1. INFORMACIÓN GENERAL",
                                contenido =>
                                {
                                    contenido.Item().Row(row =>
                                    {
                                        row.RelativeItem().Element(c =>
                                            CampoDetalle(c, "Área", registro.Area));

                                        row.RelativeItem().Element(c =>
                                            CampoDetalle(c, "Mes", NombreMes(registro.Mes)));

                                        row.RelativeItem().Element(c =>
                                            CampoDetalle(c, "Año", registro.Anio.ToString()));
                                    });

                                    contenido.Item()
                                        .PaddingTop(8)
                                        .Row(row =>
                                        {
                                            row.RelativeItem().Element(c =>
                                                CampoDetalle(c, "Fuente", registro.Fuente));

                                            row.RelativeItem().Element(c =>
                                                CampoDetalle(c, "Prioridad", registro.Prioridad));

                                            row.RelativeItem().Element(c =>
                                                CampoDetalle(c, "Estado", registro.Estado));
                                        });
                                }
                            );
                        });

                    // 2. DESCRIPCIÓN

                    col.Item()
                        .PaddingTop(12)
                        .Element(container =>
                        {
                            Card.Dibujar(
                                container,
                                "2. DESCRIPCIÓN DEL SEGUIMIENTO",
                                contenido =>
                                {
                                    contenido.Item().Element(c =>
                                        CampoDetalle(
                                            c,
                                            "Descripción",
                                            registro.Descripcion
                                        )
                                    );
                                }
                            );
                        });

                    // 3. FACTORES DE RIESGO Y PLAN DE ACCIÓN

                    col.Item()
                        .PaddingTop(12)
                        .Element(container =>
                        {
                            Card.Dibujar(
                                container,
                                "3. FACTORES DE RIESGO Y PLAN DE ACCIÓN",
                                contenido =>
                                {
                                    contenido.Item().Element(c =>
                                        CampoDetalle(
                                            c,
                                            "Factor de riesgo",
                                            registro.FactorRiesgo
                                        )
                                    );

                                    contenido.Item()
                                        .PaddingTop(8)
                                        .Element(c =>
                                            CampoDetalle(
                                                c,
                                                "Plan de acción sugerido",
                                                registro.PlanAccionSugerido
                                            )
                                        );
                                }
                            );
                        });

                    // 4. RESPONSABLE Y PROGRAMACIÓN

                    col.Item()
                        .PaddingTop(12)
                        .Element(container =>
                        {
                            Card.Dibujar(
                                container,
                                "4. RESPONSABLE Y PROGRAMACIÓN",
                                contenido =>
                                {
                                    contenido.Item().Row(row =>
                                    {
                                        row.RelativeItem().Element(c =>
                                            CampoDetalle(
                                                c,
                                                "Responsable",
                                                registro.Responsable
                                            )
                                        );

                                        row.RelativeItem().Element(c =>
                                            CampoDetalle(
                                                c,
                                                "Fecha de ejecución",
                                                registro.FechaEjecucion?.ToString("dd/MM/yyyy")
                                            )
                                        );
                                    });

                                    contenido.Item()
                                        .PaddingTop(8)
                                        .Element(c =>
                                            CampoDetalle(
                                                c,
                                                "Fecha de seguimiento",
                                                registro.FechaSeguimiento?.ToString("dd/MM/yyyy")
                                            )
                                        );
                                }
                            );
                        });

                    // 5. OBSERVACIONES

                    col.Item()
                        .PaddingTop(12)
                        .Element(container =>
                        {
                            Card.Dibujar(
                                container,
                                "5. OBSERVACIONES",
                                contenido =>
                                {
                                    contenido.Item().Element(c =>
                                        CampoDetalle(
                                            c,
                                            "Observaciones",
                                            registro.Observaciones
                                        )
                                    );
                                }
                            );
                        });

                    // 6. INFORMACIÓN DE REGISTRO

                    col.Item()
                        .PaddingTop(12)
                        .Element(container =>
                        {
                            Card.Dibujar(
                                container,
                                "6. INFORMACIÓN DE REGISTRO",
                                contenido =>
                                {
                                    contenido.Item().Row(row =>
                                    {
                                        row.RelativeItem().Element(c =>
                                            CampoDetalle(
                                                c,
                                                "Creado por",
                                                registro.CreadoPor.ToString()
                                            )
                                        );

                                        row.RelativeItem().Element(c =>
                                            CampoDetalle(
                                                c,
                                                "Fecha de creación",
                                                registro.FechaCreacion.ToString("dd/MM/yyyy HH:mm")
                                            )
                                        );
                                    });

                                    contenido.Item()
                                        .PaddingTop(8)
                                        .Row(row =>
                                        {
                                            row.RelativeItem().Element(c =>
                                                CampoDetalle(
                                                    c,
                                                    "Modificado por",
                                                    registro.ModificadoPor?.ToString()
                                                )
                                            );

                                            row.RelativeItem().Element(c =>
                                                CampoDetalle(
                                                    c,
                                                    "Fecha de modificación",
                                                    registro.FechaModificacion?.ToString("dd/MM/yyyy HH:mm")
                                                )
                                            );
                                        });
                                }
                            );
                        });

                    // 7. EVIDENCIAS FOTOGRÁFICAS

                    col.Item()
                        .PaddingTop(12)
                        .Element(container =>
                        {
                            Card.Dibujar(
                                container,
                                "7. EVIDENCIAS FOTOGRÁFICAS",
                                contenido =>
                                {
                                    var fotos = registro.Fotos.ToList();

                                    if (!fotos.Any())
                                    {
                                        contenido.Item()
                                            .Text("No hay fotografías registradas.")
                                            .FontSize(9);

                                        return;
                                    }

                                    // EVIDENCIAS

                                    var evidencias = fotos
                                        .Where(f => f.TipoFoto == "evidencia")
                                        .Select(f => new
                                        {
                                            f.NombreArchivo,
                                            Ruta = ObtenerRutaFoto(f.NombreArchivo)
                                        })
                                        .Where(f => File.Exists(f.Ruta))
                                        .ToList();

                                    if (evidencias.Any())
                                    {
                                        contenido.Item()
                                            .Text("EVIDENCIAS INICIALES")
                                            .Bold()
                                            .FontSize(10)
                                            .FontColor(PdfColors.VerdeOscuro);

                                        contenido.Item()
                                            .PaddingTop(8)
                                            .Grid(grid =>
                                            {
                                                grid.Columns(2);
                                                grid.Spacing(10);

                                                foreach (var foto in evidencias)
                                                {
                                                    grid.Item()
                                                        .Height(150)
                                                        .Border(1)
                                                        .BorderColor(PdfColors.GrisClaro)
                                                        .Padding(5)
                                                        .Image(File.ReadAllBytes(foto.Ruta))
                                                        .FitArea();
                                                }
                                            });
                                    }

                                    // FOTOGRAFÍAS DE SEGUIMIENTO

                                    var seguimientos = fotos
                                        .Where(f => f.TipoFoto == "seguimiento")
                                        .Select(f => new
                                        {
                                            f.NombreArchivo,
                                            Ruta = ObtenerRutaFoto(f.NombreArchivo)
                                        })
                                        .Where(f => File.Exists(f.Ruta))
                                        .ToList();

                                    if (seguimientos.Any())
                                    {
                                        contenido.Item()
                                            .PaddingTop(15)
                                            .Text("FOTOGRAFÍAS DE SEGUIMIENTO")
                                            .Bold()
                                            .FontSize(10)
                                            .FontColor(PdfColors.VerdeOscuro);

                                        contenido.Item()
                                            .PaddingTop(8)
                                            .Grid(grid =>
                                            {
                                                grid.Columns(2);
                                                grid.Spacing(10);

                                                foreach (var foto in seguimientos)
                                                {
                                                    grid.Item()
                                                        .Height(150)
                                                        .Border(1)
                                                        .BorderColor(PdfColors.GrisClaro)
                                                        .Padding(5)
                                                        .Image(File.ReadAllBytes(foto.Ruta))
                                                        .FitArea();
                                                }
                                            });
                                    }

                                    // AVISAR SI NO SE ENCONTRARON ARCHIVOS

                                    if (!evidencias.Any() && !seguimientos.Any())
                                    {
                                        contenido.Item()
                                            .Text("No se encontraron los archivos fotográficos en el servidor.")
                                            .FontSize(9);
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

        // CAMPO DE INFORMACIÓN

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
                    .PaddingTop(3)
                    .Text(
                        string.IsNullOrWhiteSpace(valor)
                            ? "No registrado"
                            : valor
                    )
                    .FontSize(9)
                    .FontColor(PdfColors.GrisOscuro);
            });
        }

        // RUTAS DE ARCHIVOS

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

        // NOMBRE DEL MES

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
