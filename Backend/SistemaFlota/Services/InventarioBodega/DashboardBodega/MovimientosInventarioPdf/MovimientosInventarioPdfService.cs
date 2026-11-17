
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using SistemaFlota.DTOs.InventarioBodega.DashboardBodega;
using SistemaFlota.Services.Pdf.Components;

namespace SistemaFlota.Services.InventarioBodega.DashboardBodega.MovimientosInventarioPdf
{
    public class MovimientosInventarioPdfService
        : IMovimientosInventarioPdfService
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public MovimientosInventarioPdfService(
            AppDbContext context,
            IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        public async Task<byte[]> GenerarPdfAsync(
            MovimientoInventarioFiltroDto filtros)
        {
            // CONSULTAR TODOS LOS MOVIMIENTOS FILTRADOS
            var movimientos = await ObtenerMovimientosFiltrados(filtros);

            var empresa = await _context.ConfiguracionEmpresa
                .FirstOrDefaultAsync();

            if (empresa == null)
                throw new Exception(
                    "No existe la configuración de la empresa."
                );

            var totalPeso = movimientos.Sum(x => x.Peso);

            var pdf = Document.Create(document =>
            {
                document.Page(page =>
                {
                    page.Size(PageSizes.A4.Landscape());
                    page.Margin(20);

                    page.DefaultTextStyle(x => x.FontSize(8));

                    ConstruirDocumento(
                        page,
                        movimientos,
                        empresa,
                        filtros,
                        totalPeso
                    );
                });
            });

            return pdf.GeneratePdf();
        }

        // CONSULTAR MOVIMIENTOS CON LOS FILTROS

        private async Task<List<MovimientoInventarioAuditoriaDto>>
            ObtenerMovimientosFiltrados(
                MovimientoInventarioFiltroDto filtros)
        {
            var query = _context.MovimientosInventarioBodega
                .AsNoTracking()
                .AsQueryable();

            // FECHA INICIAL
            if (filtros.FechaInicio.HasValue)
            {
                var fechaInicio = filtros.FechaInicio.Value
                    .Date
                    .ToUniversalTime();

                query = query.Where(m =>
                    m.Fecha >= fechaInicio
                );
            }

            // FECHA FINAL
            if (filtros.FechaFin.HasValue)
            {
                var fechaFin = filtros.FechaFin.Value
                    .Date
                    .AddDays(1)
                    .AddTicks(-1)
                    .ToUniversalTime();

                query = query.Where(m =>
                    m.Fecha <= fechaFin
                );
            }

            // REFERENCIA
            if (!string.IsNullOrWhiteSpace(filtros.Referencia))
            {
                var referencia = filtros.Referencia.Trim();

                query = query.Where(m =>
                    m.Referencia.Contains(referencia)
                );
            }

            // USUARIO
            if (!string.IsNullOrWhiteSpace(filtros.Usuario))
            {
                var usuario = filtros.Usuario.Trim();

                query = query.Where(m =>
                    m.Usuario != null &&
                    m.Usuario.Contains(usuario)
                );
            }

            // CLIENTE
            if (!string.IsNullOrWhiteSpace(filtros.Cliente))
            {
                var cliente = filtros.Cliente.Trim();

                query = query.Where(m =>
                    m.Cliente != null &&
                    m.Cliente.Nombre.Contains(cliente)
                );
            }

            // TIPO
            if (!string.IsNullOrWhiteSpace(filtros.Tipo))
            {
                var tipo = filtros.Tipo.Trim();

                query = query.Where(m =>
                    m.Tipo.Contains(tipo)
                );
            }

            // REFERENCIA IMPRESA
            if (!string.IsNullOrWhiteSpace(filtros.ReferenciaImpresa))
            {
                var referenciaImpresa =
                    filtros.ReferenciaImpresa.Trim();

                query = query.Where(m =>
                    m.InventarioBodega != null &&
                    m.InventarioBodega.ReferenciaImpresa != null &&
                    m.InventarioBodega.ReferenciaImpresa
                        .Contains(referenciaImpresa)
                );
            }

            // EL PDF DEBE INCLUIR TODOS LOS RESULTADOS.

            return await query
                .OrderByDescending(m => m.Fecha)
                .ThenByDescending(m => m.Id)
                .Select(m => new MovimientoInventarioAuditoriaDto
                {
                    Referencia = m.Referencia,
                    ReferenciaPeso = m.ReferenciaPeso,
                    Tipo = m.Tipo,
                    Peso = m.Peso,
                    Fecha = m.Fecha,

                    Usuario = m.Usuario ?? "Sin usuario",

                    Cliente = m.Cliente != null
                        ? m.Cliente.Nombre
                        : null,

                    ReferenciaImpresa =
                        m.InventarioBodega != null
                            ? m.InventarioBodega.ReferenciaImpresa
                            : null,

                    Observacion = m.Observacion
                })
                .ToListAsync();
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

        private string ObtenerLogo(string? logo)
        {
            return Path.Combine(
                _environment.ContentRootPath,
                "wwwroot",
                "config",
                "logo.png"
            );
        }

        private void ConstruirDocumento(
            PageDescriptor page,
            List<MovimientoInventarioAuditoriaDto> movimientos,
            ConfiguracionEmpresa empresa,
            MovimientoInventarioFiltroDto filtros,
            decimal totalPeso)
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
                                "AUDITORÍA DE INVENTARIO",
                                "F-GC-027 V2",
                                "MOVIMIENTOS DE INVENTARIO",
                                $"Total registros: {movimientos.Count}"
                            );
                        });

                    // FILTROS 

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
                                            $"Fecha inicial: {filtros.FechaInicio?.ToString("dd/MM/yyyy") ?? "Todas"}"
                                        );

                                        row.RelativeItem().Text(
                                            $"Fecha final: {filtros.FechaFin?.ToString("dd/MM/yyyy") ?? "Todas"}"
                                        );

                                        row.RelativeItem().Text(
                                            $"Tipo: {filtros.Tipo ?? "Todos"}"
                                        );
                                    });

                                    contenido.Item().Row(row =>
                                    {
                                        row.RelativeItem().Text(
                                            $"Referencia: {filtros.Referencia ?? "Todas"}"
                                        );

                                        row.RelativeItem().Text(
                                            $"Usuario: {filtros.Usuario ?? "Todos"}"
                                        );

                                        row.RelativeItem().Text(
                                            $"Cliente: {filtros.Cliente ?? "Todos"}"
                                        );
                                    });

                                    if (!string.IsNullOrWhiteSpace(
                                        filtros.ReferenciaImpresa))
                                    {
                                        contenido.Item().Text(
                                            $"Referencia impresa: {filtros.ReferenciaImpresa}"
                                        );
                                    }
                                }
                            );
                        });

                    // RESUMEN

                    col.Item()
                        .PaddingTop(10)
                        .Row(row =>
                        {
                            row.RelativeItem()
                                .Border(1)
                                .BorderColor(Colors.Grey.Lighten2)
                                .Padding(8)
                                .Column(c =>
                                {
                                    c.Item().Text("TOTAL MOVIMIENTOS")
                                        .FontSize(8)
                                        .FontColor(Colors.Grey.Darken1);

                                    c.Item().Text(
                                        movimientos.Count.ToString("N0")
                                    )
                                    .Bold()
                                    .FontSize(15);
                                });

                            row.ConstantItem(12);

                            row.RelativeItem()
                                .Border(1)
                                .BorderColor(Colors.Grey.Lighten2)
                                .Padding(8)
                                .Column(c =>
                                {
                                    c.Item().Text("PESO TOTAL")
                                        .FontSize(8)
                                        .FontColor(Colors.Grey.Darken1);

                                    c.Item().Text(
                                        $"{totalPeso:N2} kg"
                                    )
                                    .Bold()
                                    .FontSize(15);
                                });
                        });

                    // TABLA DE MOVIMIENTOS

                    col.Item()
                        .PaddingTop(12)
                        .Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(1.2f); // Fecha
                                columns.RelativeColumn(1.2f); // Referencia
                                columns.RelativeColumn(1.2f); // Ref peso
                                columns.RelativeColumn(1.0f); // Tipo
                                columns.RelativeColumn(0.8f); // Peso
                                columns.RelativeColumn(1.2f); // Usuario
                                columns.RelativeColumn(1.2f); // Cliente
                                columns.RelativeColumn(1.2f); // Ref impresa
                                columns.RelativeColumn(2.0f); // Observación
                            });

                            // ENCABEZADOS DE TABLA

                            table.Header(header =>
                            {
                                CrearEncabezado(header, "FECHA");
                                CrearEncabezado(header, "REFERENCIA");
                                CrearEncabezado(header, "REF. PESO");
                                CrearEncabezado(header, "TIPO");
                                CrearEncabezado(header, "PESO KG");
                                CrearEncabezado(header, "USUARIO");
                                CrearEncabezado(header, "CLIENTE");
                                CrearEncabezado(header, "REF. IMPRESA");
                                CrearEncabezado(header, "OBSERVACIÓN");
                            });

                            // REGISTROS
                            foreach (var movimiento in movimientos)
                            {
                                CrearCelda(
                                    table,
                                    movimiento.Fecha.ToString("dd/MM/yyyy HH:mm")
                                );

                                CrearCelda(
                                    table,
                                    movimiento.Referencia
                                );

                                CrearCelda(
                                    table,
                                    movimiento.ReferenciaPeso
                                );

                                CrearCelda(
                                    table,
                                    movimiento.Tipo
                                );

                                CrearCelda(
                                    table,
                                    movimiento.Peso.ToString("N2")
                                );

                                CrearCelda(
                                    table,
                                    movimiento.Usuario
                                );

                                CrearCelda(
                                    table,
                                    movimiento.Cliente ?? "-"
                                );

                                CrearCelda(
                                    table,
                                    movimiento.ReferenciaImpresa ?? "-"
                                );

                                CrearCelda(
                                    table,
                                    movimiento.Observacion ?? "-"
                                );
                            }
                        });

                    // TOTAL FINAL

                    col.Item()
                        .PaddingTop(8)
                        .AlignRight()
                        .Text(
                            $"TOTAL PESO: {totalPeso:N2} KG"
                        )
                        .Bold()
                        .FontSize(11);
                });

            page.Footer()
                .Element(container =>
                    FooterEmpresa.Dibujar(container)
                );
        }

        // CELDA DE ENCABEZADO

        private void CrearEncabezado(
            TableCellDescriptor header,
            string texto)
        {
            header.Cell()
                .Background(Colors.Grey.Darken3)
                .Padding(5)
                .AlignCenter()
                .Text(texto)
                .FontColor(Colors.White)
                .Bold()
                .FontSize(7);
        }

        // CELDA DE DATOS

        private void CrearCelda(
            TableDescriptor table,
            string texto)
        {
            table.Cell()
                .BorderBottom(0.5f)
                .BorderColor(Colors.Grey.Lighten2)
                .Padding(4)
                .Text(texto)
                .FontSize(7);
        }
    }
}
