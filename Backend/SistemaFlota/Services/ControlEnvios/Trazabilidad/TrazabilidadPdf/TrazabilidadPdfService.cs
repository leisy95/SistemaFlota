using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SistemaFlota.Services.Auth;
using SistemaFlota.Services.Pdf.Components;
using SistemaFlota.Services.Pdf.Styles;
using System.Globalization;

namespace SistemaFlota.Services.ControlEnvios.Trazabilidad.TrazabilidadPdf
{
    public class TrazabilidadPdfService : ITrazabilidadPdfService
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _environment;
        private readonly ICurrentUserService _currentUser;

        public TrazabilidadPdfService(
            AppDbContext context,
            IWebHostEnvironment environment,
            ICurrentUserService currentUser)
        {
            _context = context;
            _environment = environment;
            _currentUser = currentUser;
        }

        public async Task<byte[]> GenerarPdfAsync(
            string? buscar,
            string? estado,
            string? entregada,
            string? tipo,
            DateTime? fechaDesde,
            DateTime? fechaHasta)
        {
            // DATOS DEL USUARIO QUE GENERA EL REPORTE
            var usuarioGenerador =
                _currentUser.Nombre
                ?? _currentUser.Usuario
                ?? _currentUser.Email
                ?? "Desconocido";

            var cargoGenerador =
                _currentUser.Rol
                ?? "No especificado";

            var query = _context.TrazabilidadFacturas
                .AsNoTracking()
                .AsQueryable();

            // BÚSQUEDA POR FACTURA, CLIENTE, CONDUCTOR,
            // VEHÍCULO O GUÍA
            if (!string.IsNullOrWhiteSpace(buscar))
            {
                var q = buscar.Trim().ToLower();

                query = query.Where(t =>
                    t.FacturaRemision.ToLower().Contains(q) ||
                    t.Cliente.ToLower().Contains(q) ||
                    t.Conductor.ToLower().Contains(q) ||
                    (t.Vehiculo != null &&
                     t.Vehiculo.ToLower().Contains(q)) ||
                    (t.Guia != null &&
                     t.Guia.ToLower().Contains(q)));
            }

            // FILTRO POR ESTADO
            if (!string.IsNullOrWhiteSpace(estado))
            {
                query = query.Where(t => t.Estado == estado);
            }

            // FILTRO POR ENTREGA
            if (entregada == "si")
            {
                query = query.Where(t => t.FacturaEntregada);
            }
            else if (entregada == "no")
            {
                query = query.Where(t => !t.FacturaEntregada);
            }

            // FILTRO POR TIPO DE DOCUMENTO
            if (!string.IsNullOrWhiteSpace(tipo))
            {
                var tipoUpper = tipo.Trim().ToUpper();

                var alternativo = tipoUpper switch
                {
                    "CT" => "COT",
                    "RM" => "RE",
                    "NCE" => "NC",
                    "COT" => "CT",
                    "RE" => "RM",
                    "NC" => "NCE",
                    _ => ""
                };

                query = query.Where(t =>
                    t.FacturaRemision.ToUpper().StartsWith(tipoUpper) ||
                    (alternativo != "" &&
                     t.FacturaRemision.ToUpper().StartsWith(alternativo)));
            }

            // FECHA DESDE: INCLUYE TODO EL DÍA
            if (fechaDesde.HasValue)
            {
                var desde = fechaDesde.Value.Date;

                query = query.Where(t =>
                    t.FechaRegistro >= desde);
            }

            // FECHA HASTA: INCLUYE TODO EL DÍA
            if (fechaHasta.HasValue)
            {
                var hastaExclusiva = fechaHasta.Value.Date.AddDays(1);

                query = query.Where(t =>
                    t.FechaRegistro < hastaExclusiva);
            }

            // OBTENER TODOS LOS REGISTROS FILTRADOS
            var registros = await query
                .OrderByDescending(t => t.FechaRegistro)
                .ThenByDescending(t => t.Id)
                .ToListAsync();

            // CONFIGURACIÓN DE LA EMPRESA
            var empresa = await _context.ConfiguracionEmpresa
                .AsNoTracking()
                .FirstOrDefaultAsync();

            if (empresa == null)
            {
                throw new InvalidOperationException(
                    "No existe la configuración de la empresa.");
            }

            // RESUMEN DEL REPORTE
            var total = registros.Count;

            var totalEntregadas = registros.Count(
                t => t.FacturaEntregada);

            var totalPendientes = total - totalEntregadas;

            var totalFlete = registros.Sum(
                t => t.ValorFlete ?? 0m);

            // RUTAS DE LOS RECURSOS
            var rutaLogo = Path.Combine(
                _environment.ContentRootPath,
                "wwwroot",
                "config",
                "logo.png");

            var rutaMarcaAgua = Path.Combine(
                _environment.ContentRootPath,
                "wwwroot",
                "config",
                "iguana3.png");

            // GENERAR PDF
            var pdf = Document.Create(document =>
            {
                document.Page(page =>
                {
                    page.Size(PageSizes.A4.Landscape());
                    page.Margin(20);

                    page.DefaultTextStyle(x => x.FontSize(8));

                    // MARCA DE AGUA EN TODAS LAS PÁGINAS
                    page.Background().Element(container =>
                        MarcaAgua.Dibujar(
                            container,
                            rutaMarcaAgua));

                    // CONTENIDO PRINCIPAL
                    // IMPORTANTE: UNA SOLA COLUMNA PRINCIPAL.
                    // El encabezado se dibuja al principio del contenido,
                    // no en page.Header(), por lo que no se repite.
                    page.Content().Column(content =>
                    {
                        content.Spacing(10);

                        // ENCABEZADO DE EMPRESA: UNA SOLA VEZ
                        content.Item()
                            .PaddingBottom(5)
                            .Element(container =>
                                HeaderEmpresa.Dibujar(
                                    container,
                                    rutaLogo,
                                    empresa,
                                    "F-GC-027 V2",
                                    "",
                                    "REPORTE DE TRAZABILIDAD",
                                    DateTime.Now.ToString("dd/MM/yyyy HH:mm")
                                )
                            );

                        // FILTROS Y DATOS DEL GENERADOR
                        content.Item()
                            .Border(1)
                            .BorderColor(PdfColors.GrisClaro)
                            .Padding(8)
                            .Row(row =>
                            {
                                // COLUMNA IZQUIERDA: FILTROS
                                row.RelativeItem(3)
                                    .Column(col =>
                                    {
                                        col.Spacing(4);

                                        col.Item()
                                            .Text("FILTROS DEL REPORTE")
                                            .Bold()
                                            .FontSize(10)
                                            .FontColor(PdfColors.VerdeOscuro);

                                        col.Item().Text(
                                            $"Búsqueda: {ValorFiltro(buscar)}");

                                        col.Item().Text(
                                            $"Estado: {ValorFiltro(estado)}" +
                                            $"   |   Entrega: {ValorFiltro(entregada)}");

                                        col.Item().Text(
                                            $"Tipo: {ValorFiltro(tipo)}");

                                        col.Item().Text(
                                            $"Desde: {FormatoFecha(fechaDesde)}" +
                                            $"   |   Hasta: {FormatoFecha(fechaHasta)}");
                                    });

                                // SEPARADOR VERTICAL
                                row.ConstantItem(1)
                                    .Background(PdfColors.GrisClaro);

                                // COLUMNA DERECHA: USUARIO Y CARGO
                                row.RelativeItem(1)
                                    .PaddingLeft(12)
                                    .Column(col =>
                                    {
                                        col.Spacing(6);

                                        col.Item()
                                            .Text("GENERADO POR")
                                            .Bold()
                                            .FontSize(10)
                                            .FontColor(PdfColors.VerdeOscuro);

                                        col.Item()
                                            .Text("Usuario:")
                                            .FontSize(8)
                                            .FontColor(PdfColors.GrisMedio);

                                        col.Item()
                                            .Text(usuarioGenerador)
                                            .Bold()
                                            .FontSize(9);

                                        col.Item()
                                            .Text("Cargo:")
                                            .FontSize(8)
                                            .FontColor(PdfColors.GrisMedio);

                                        col.Item()
                                            .Text(cargoGenerador)
                                            .Bold()
                                            .FontSize(9);
                                    });
                            });

                        // TARJETAS DE RESUMEN
                        content.Item().Row(row =>
                        {
                            Resumen(
                                row,
                                "TOTAL REGISTROS",
                                total.ToString("N0"));

                            Resumen(
                                row,
                                "FACTURAS ENTREGADAS",
                                totalEntregadas.ToString("N0"));

                            Resumen(
                                row,
                                "PENDIENTES ENTREGA",
                                totalPendientes.ToString("N0"));

                            Resumen(
                                row,
                                "TOTAL FLETE",
                                totalFlete.ToString(
                                    "C0",
                                    CultureInfo.GetCultureInfo("es-CO")));
                        });

                        content.Item()
                            .Text($"REGISTROS DE TRAZABILIDAD ({total})")
                            .Bold()
                            .FontSize(11);

                        // MENSAJE CUANDO NO HAY RESULTADOS
                        if (total == 0)
                        {
                            content.Item()
                                .Padding(10)
                                .AlignCenter()
                                .Text(
                                    "No se encontraron registros " +
                                    "con los filtros seleccionados.")
                                .Italic();
                        }
                        else
                        {
                            // TABLA PRINCIPAL
                            content.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(1.3f); // Factura
                                    columns.RelativeColumn(1.8f); // Cliente
                                    columns.RelativeColumn(1.4f); // Conductor
                                    columns.RelativeColumn(1f);   // Vehículo
                                    columns.RelativeColumn(1.4f); // Transportadora
                                    columns.RelativeColumn(1f);   // Guía
                                    columns.RelativeColumn(1.2f); // Fecha
                                    columns.RelativeColumn(1f);   // Estado
                                    columns.RelativeColumn(0.8f); // Entregada
                                    columns.RelativeColumn(1.2f); // Flete
                                });

                                // CABECERA DE TABLA
                                // QuestPDF la repetirá automáticamente
                                // cuando la tabla continúe en otra página.
                                table.Header(header =>
                                {
                                    CeldaCabecera(header, "Factura");
                                    CeldaCabecera(header, "Cliente");
                                    CeldaCabecera(header, "Conductor");
                                    CeldaCabecera(header, "Vehículo");
                                    CeldaCabecera(header, "Transportadora");
                                    CeldaCabecera(header, "Guía");
                                    CeldaCabecera(header, "Fecha registro");
                                    CeldaCabecera(header, "Estado");
                                    CeldaCabecera(header, "Entregada");
                                    CeldaCabecera(header, "Flete");
                                });

                                // FILAS DE LA TABLA
                                foreach (var item in registros)
                                {
                                    CeldaCuerpo(
                                        table,
                                        item.FacturaRemision ?? "-");

                                    CeldaCuerpo(
                                        table,
                                        item.Cliente ?? "-");

                                    CeldaCuerpo(
                                        table,
                                        item.Conductor ?? "-");

                                    CeldaCuerpo(
                                        table,
                                        item.Vehiculo ?? "-");

                                    CeldaCuerpo(
                                        table,
                                        item.Transportadora ?? "-");

                                    CeldaCuerpo(
                                        table,
                                        item.Guia ?? "-");

                                    CeldaCuerpo(
                                        table,
                                        item.FechaRegistro.ToString(
                                            "dd/MM/yyyy HH:mm"));

                                    CeldaEstado(
                                        table,
                                        item.Estado ?? "-");

                                    CeldaCuerpo(
                                        table,
                                        item.FacturaEntregada ? "Sí" : "No");

                                    CeldaCuerpo(
                                        table,
                                        (item.ValorFlete ?? 0m).ToString(
                                            "N0",
                                            CultureInfo.GetCultureInfo("es-CO")));
                                }
                            });
                        }
                    });

                    // PIE DE PÁGINA CORPORATIVO
                    // Se conserva en todas las páginas.
                    page.Footer()
                        .Element(FooterEmpresa.Dibujar);
                });
            });

            return pdf.GeneratePdf();
        }

        private static string ValorFiltro(string? valor)
        {
            return string.IsNullOrWhiteSpace(valor)
                ? "Todos"
                : valor.Trim();
        }

        private static string FormatoFecha(DateTime? fecha)
        {
            return fecha.HasValue
                ? fecha.Value.ToString("dd/MM/yyyy")
                : "No especificada";
        }

        private static void Resumen(
            RowDescriptor row,
            string titulo,
            string valor)
        {
            row.RelativeItem()
                .Border(1)
                .BorderColor(PdfColors.GrisClaro)
                .Background(PdfColors.Fondo)
                .Padding(8)
                .Column(col =>
                {
                    col.Item()
                        .Text(titulo)
                        .FontSize(7)
                        .FontColor(PdfColors.GrisMedio);

                    col.Item()
                        .Text(valor)
                        .FontSize(12)
                        .Bold()
                        .FontColor(PdfColors.VerdeOscuro);
                });
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
                .Padding(4)
                .Text(texto)
                .FontSize(7);
        }

        private static void CeldaEstado(
            TableDescriptor table,
            string estado)
        {
            var color = estado.ToLowerInvariant() switch
            {
                "entregada" => PdfColors.VerdeOscuro,
                "pendiente" => PdfColors.Amarillo,
                _ => PdfColors.AzulOscuro
            };

            table.Cell()
                .BorderBottom(1)
                .BorderColor(PdfColors.GrisClaro)
                .Padding(4)
                .Text(estado)
                .FontSize(7)
                .Bold()
                .FontColor(color);
        }
    }
}