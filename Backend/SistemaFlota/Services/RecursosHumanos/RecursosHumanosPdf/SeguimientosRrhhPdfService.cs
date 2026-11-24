using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SistemaFlota.Models;
using SistemaFlota.Services.Pdf.Components;
using SistemaFlota.Services.Pdf.Styles;

namespace SistemaFlota.Services.RecursosHumanos.RecursosHumanosPdf
{
    public class SeguimientosRrhhPdfService
       : ISeguimientosRrhhPdfService
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public SeguimientosRrhhPdfService(
            AppDbContext context,
            IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        public async Task<byte[]> GenerarPdfAsync(
            string? area,
            string? estado,
            string? prioridad,
            int? mes,
            int? anio)
        {
            var registros = await ObtenerRegistrosFiltrados(
                area,
                estado,
                prioridad,
                mes,
                anio
            );

            var empresa = await _context.ConfiguracionEmpresa
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
                    page.Margin(20);

                    page.DefaultTextStyle(x => x.FontSize(8));

                    ConstruirDocumento(
                        page,
                        registros,
                        empresa,
                        area,
                        estado,
                        prioridad,
                        mes,
                        anio
                    );
                });
            });

            return pdf.GeneratePdf();
        }

        // CONSULTAR REGISTROS FILTRADOS

        private async Task<List<SeguimientoRrhh>> ObtenerRegistrosFiltrados(
            string? area,
            string? estado,
            string? prioridad,
            int? mes,
            int? anio)
        {
            var query = _context.SeguimientosRrhh
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(area))
            {
                var valor = area.Trim();

                query = query.Where(s =>
                    s.Area.Contains(valor)
                );
            }

            if (!string.IsNullOrWhiteSpace(estado))
            {
                query = query.Where(s =>
                    s.Estado == estado
                );
            }

            if (!string.IsNullOrWhiteSpace(prioridad))
            {
                query = query.Where(s =>
                    s.Prioridad == prioridad
                );
            }

            if (mes.HasValue && mes.Value > 0)
            {
                query = query.Where(s =>
                    s.Mes == mes.Value
                );
            }

            if (anio.HasValue && anio.Value > 0)
            {
                query = query.Where(s =>
                    s.Anio == anio.Value
                );
            }

            return await query
                .OrderByDescending(s => s.FechaCreacion)
                .ThenByDescending(s => s.Id)
                .ToListAsync();
        }

        // RUTAS DE IMÁGENES

        private string ObtenerMarcaAgua()
        {
            return Path.Combine(
                _environment.ContentRootPath,
                "wwwroot",
                "config",
                "iguana3.png"
            );
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

        // CONSTRUIR DOCUMENTO

        private void ConstruirDocumento(
            PageDescriptor page,
            List<SeguimientoRrhh> registros,
            ConfiguracionEmpresa empresa,
            string? area,
            string? estado,
            string? prioridad,
            int? mes,
            int? anio)
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
                .PaddingTop(12)
                .Column(col =>
                {
                    // ENCABEZADO EMPRESA

                    col.Item()
                         .Element(container =>
                         {
                             HeaderEmpresa.Dibujar(
                                 container,
                                 ObtenerLogo(empresa.Logo),
                                 empresa,
                                 "SEGUIMIENTO DE MATRIZ DE MEJORA",
                                 "F-SST-74 V2",
                                 "",
                                 "15-09-25"
                             );
                         });

                    // FILTROS APLICADOS

                    col.Item()
                        .PaddingTop(10)
                        .Element(container =>
                        {
                            Card.Dibujar(
                                container,
                                "FILTROS APLICADOS",
                                contenido =>
                                {
                                    contenido.Item().Row(row =>
                                    {
                                        row.RelativeItem().Text(
                                            $"Área: {area ?? "Todas"}"
                                        );

                                        row.RelativeItem().Text(
                                            $"Estado: {estado ?? "Todos"}"
                                        );

                                        row.RelativeItem().Text(
                                            $"Prioridad: {prioridad ?? "Todas"}"
                                        );
                                    });

                                    contenido.Item().Row(row =>
                                    {
                                        row.RelativeItem().Text(
                                            $"Mes: {(mes.HasValue && mes > 0 ? NombreMes(mes.Value) : "Todos")}"
                                        );

                                        row.RelativeItem().Text(
                                            $"Año: {(anio.HasValue && anio > 0 ? anio.ToString() : "Todos")}"
                                        );

                                        row.RelativeItem().Text(
                                            $"Fecha generación: {DateTime.Now:dd/MM/yyyy}"
                                        );
                                    });
                                }
                            );
                        });

                    // RESUMEN

                    col.Item()
                        .PaddingTop(10)
                        .Row(row =>
                        {
                            CrearResumen(
                                row.RelativeItem(),
                                "TOTAL SEGUIMIENTOS",
                                registros.Count.ToString("N0")
                            );

                            row.ConstantItem(10);

                            CrearResumen(
                                row.RelativeItem(),
                                "ABIERTAS",
                                registros.Count(x => x.Estado == "Abierta").ToString("N0")
                            );

                            row.ConstantItem(10);

                            CrearResumen(
                                row.RelativeItem(),
                                "EN PROCESO",
                                registros.Count(x => x.Estado == "En proceso").ToString("N0")
                            );

                            row.ConstantItem(10);

                            CrearResumen(
                                row.RelativeItem(),
                                "EJECUTADAS",
                                registros.Count(x => x.Estado == "Ejecutada").ToString("N0")
                            );
                        });

                    // TABLA

                    col.Item()
                        .PaddingTop(12)
                        .Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(1.1f);   // ÁREA
                                columns.RelativeColumn(1.0f);   // MES / AÑO
                                columns.RelativeColumn(1.25f);  // FUENTE
                                columns.RelativeColumn(4.0f);   // DESCRIPCIÓN
                                columns.RelativeColumn(1.15f);  // FACTOR RIESGO
                                columns.RelativeColumn(0.95f);  // PRIORIDAD
                                columns.RelativeColumn(1.25f);  // RESPONSABLE
                                columns.RelativeColumn(1.05f);  // F. EJECUCIÓN
                                columns.RelativeColumn(1.0f);   // ESTADO
                            });

                            // ENCABEZADOS

                            table.Header(header =>
                            {
                                CrearEncabezado(header, "ÁREA");
                                CrearEncabezado(header, "MES / AÑO");
                                CrearEncabezado(header, "FUENTE");
                                CrearEncabezado(header, "DESCRIPCIÓN");
                                CrearEncabezado(header, "FACTOR RIESGO");
                                CrearEncabezado(header, "PRIORIDAD");
                                CrearEncabezado(header, "RESPONSABLE");
                                CrearEncabezado(header, "F. EJECUCIÓN");
                                CrearEncabezado(header, "ESTADO");
                            });

                            // REGISTROS

                            foreach (var r in registros)
                            {
                                CrearCelda(table, r.Area);
                                CrearCelda(table, $"{NombreMes(r.Mes)} {r.Anio}");
                                CrearCelda(table, r.Fuente);
                                CrearCelda(table, r.Descripcion);
                                CrearCelda(table, r.FactorRiesgo ?? "-");

                                CrearCeldaColor(
                                    table,
                                    r.Prioridad,
                                    ObtenerColorPrioridad(r.Prioridad)
                                );

                                CrearCelda(table, r.Responsable ?? "-");

                                CrearCelda(
                                    table,
                                    r.FechaEjecucion?.ToString("dd/MM/yyyy") ?? "-"
                                );

                                CrearCeldaColor(
                                    table,
                                    r.Estado,
                                    ObtenerColorEstado(r.Estado)
                                );
                            }
                        });

                    // RESUMEN FINAL

                    col.Item()
                        .PaddingTop(10)
                        .AlignRight()
                        .Text(
                            $"TOTAL REGISTROS: {registros.Count:N0}"
                        )
                        .Bold()
                        .FontSize(11);
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

        // TARJETAS DE RESUMEN

        private void CrearResumen(
            IContainer container,
            string titulo,
            string valor)
        {
            container
                .Border(1)
                .BorderColor(Colors.Grey.Lighten2)
                .Padding(8)
                .Column(c =>
                {
                    c.Item()
                        .Text(titulo)
                        .FontSize(7)
                        .FontColor(Colors.Grey.Darken1);

                    c.Item()
                        .Text(valor)
                        .Bold()
                        .FontSize(15);
                });
        }

        // ENCABEZADO TABLA

        private void CrearEncabezado(
            TableCellDescriptor header,
            string texto)
        {
            header.Cell()
                .Background(PdfColors.VerdeOscuro)
                .Padding(5)
                .AlignCenter()
                .Text(texto)
                .FontColor(Colors.White)
                .Bold()
                .FontSize(7);
        }

        // CELDA NORMAL

        private void CrearCelda(
            TableDescriptor table,
            string? texto)
        {
            table.Cell()
                .BorderBottom(0.5f)
                .BorderColor(Colors.Grey.Lighten2)
                .Padding(4)
                .Text(texto ?? "-")
                .FontSize(7);
        }

        // CELDA CON COLOR

        private void CrearCeldaColor(
            TableDescriptor table,
            string texto,
            string color)
        {
            table.Cell()
                .BorderBottom(0.5f)
                .BorderColor(Colors.Grey.Lighten2)
                .Padding(4)
                .Text(texto)
                .Bold()
                .FontSize(7)
                .FontColor(color);
        }

        // COLORES DE PRIORIDAD

        private string ObtenerColorPrioridad(string prioridad)
        {
            return prioridad switch
            {
                "Alta" => Colors.Red.Darken1,
                "Media" => Colors.Orange.Darken2,
                "Baja" => Colors.Grey.Darken1,
                _ => Colors.Black
            };
        }

        // COLORES DE ESTADO

        private string ObtenerColorEstado(string estado)
        {
            return estado switch
            {
                "Ejecutada" => Colors.Green.Darken2,
                "En proceso" => Colors.Blue.Darken1,
                "Abierta" => Colors.Orange.Darken2,
                _ => Colors.Black
            };
        }

        // NOMBRE DEL MES

        private string NombreMes(int mes)
        {
            return mes >= 1 && mes <= 12
                ? new System.Globalization.CultureInfo("es-CO")
                    .DateTimeFormat.GetMonthName(mes)
                    .ToUpperInvariant()
                : mes.ToString();
        }
    }
}
