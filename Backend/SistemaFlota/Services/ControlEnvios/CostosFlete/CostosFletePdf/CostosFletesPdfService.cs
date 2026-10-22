
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SistemaFlota.Services.Auth;
using SistemaFlota.Services.ControlEnvios.CostosFlete.CostosFletePdf;
using SistemaFlota.Services.Pdf.Components;
using SistemaFlota.Services.Pdf.Styles;
using System.Globalization;

namespace SistemaFlota.Services.ControlEnvios.CostosFletesPdf
{
    public class CostosFletesPdfService : ICostosFletesPdfService
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _environment;
        private readonly ICurrentUserService _currentUser;

        public CostosFletesPdfService(
            AppDbContext context,
            IWebHostEnvironment environment,
            ICurrentUserService currentUser)
        {
            _context = context;
            _environment = environment;
            _currentUser = currentUser;
        }

        public async Task<byte[]> GenerarPdfAsync(
            string? desde,
            string? hasta,
            string? conductor,
            string? estado,
            string? ciudad,
            string? placa)
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

            // CONSULTA BASE
            // Se conservan las relaciones utilizadas por el listado.
            var query = _context.CostosFletes
                .AsNoTracking()
                .Include(c => c.Autorizacion)
                    .ThenInclude(a => a!.Conductor)
                .Include(c => c.Autorizacion)
                    .ThenInclude(a => a!.Vehiculo)
                .Include(c => c.Trazabilidad)
                .AsQueryable();

            // FILTRO POR FECHA INICIAL
            if (!string.IsNullOrWhiteSpace(desde))
            {
                if (!DateTime.TryParseExact(
                    desde,
                    "yyyy-MM-dd",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out var fechaDesde))
                {
                    throw new ArgumentException(
                        "La fecha inicial debe tener formato yyyy-MM-dd.");
                }

                var inicio = fechaDesde.Date;

                query = query.Where(c =>
                    c.FechaRegistro >= inicio);
            }

            // FILTRO POR FECHA FINAL
            // Incluye todos los registros del día seleccionado.
            if (!string.IsNullOrWhiteSpace(hasta))
            {
                if (!DateTime.TryParseExact(
                    hasta,
                    "yyyy-MM-dd",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out var fechaHasta))
                {
                    throw new ArgumentException(
                        "La fecha final debe tener formato yyyy-MM-dd.");
                }

                var limiteExclusivo = fechaHasta.Date.AddDays(1);

                query = query.Where(c =>
                    c.FechaRegistro < limiteExclusivo);
            }

            // FILTRO POR CONDUCTOR
            if (!string.IsNullOrWhiteSpace(conductor))
            {
                var filtro = conductor.Trim();

                query = query.Where(c =>
                    (c.Autorizacion != null &&
                     c.Autorizacion.Conductor != null &&
                     c.Autorizacion.Conductor.Nombre.Contains(filtro))
                    ||
                    (c.Trazabilidad != null &&
                     c.Trazabilidad.Conductor.Contains(filtro)));
            }

            // FILTRO POR PLACA
            if (!string.IsNullOrWhiteSpace(placa))
            {
                var filtro = placa.Trim();

                query = query.Where(c =>
                    (c.Autorizacion != null &&
                     c.Autorizacion.Vehiculo != null &&
                     c.Autorizacion.Vehiculo.Placa.Contains(filtro))
                    ||
                    (c.Trazabilidad != null &&
                     c.Trazabilidad.Vehiculo != null &&
                     c.Trazabilidad.Vehiculo.Contains(filtro)));
            }

            // FILTRO POR ESTADO
            if (!string.IsNullOrWhiteSpace(estado))
            {
                var filtro = estado.Trim();

                query = query.Where(c =>
                    c.Estado == filtro);
            }

            // FILTRO POR CIUDAD
            if (!string.IsNullOrWhiteSpace(ciudad))
            {
                var filtro = ciudad.Trim();

                query = query.Where(c =>
                    c.Autorizacion != null &&
                    c.Autorizacion.DestinoCompleto.Contains(filtro));
            }

            // OBTENER TODOS LOS REGISTROS FILTRADOS
            var registros = await query
                .OrderByDescending(c => c.FechaRegistro)
                .ThenByDescending(c => c.Id)
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

            var totalPendientes = registros.Count(
                c => c.Estado == "Pendiente");

            var totalVerificados = registros.Count(
                c => c.Estado == "Verificado");

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
                    page.Content().Column(content =>
                    {
                        content.Spacing(10);

                        // ENCABEZADO DE EMPRESA
                        content.Item()
                            .PaddingBottom(5)
                            .Element(container =>
                                HeaderEmpresa.Dibujar(
                                    container,
                                    rutaLogo,
                                    empresa,
                                    "F-GC-027 V2",
                                    "",
                                    "REPORTE DE COSTOS DE FLETES",
                                    DateTime.Now.ToString(
                                        "dd/MM/yyyy HH:mm")
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
                                            .FontColor(
                                                PdfColors.VerdeOscuro);

                                        col.Item().Text(
                                            $"Desde: {FormatoFecha(desde)}" +
                                            $"   |   Hasta: {FormatoFecha(hasta)}");

                                        col.Item().Text(
                                            $"Conductor: {ValorFiltro(conductor)}" +
                                            $"   |   Placa: {ValorFiltro(placa)}");

                                        col.Item().Text(
                                            $"Ciudad: {ValorFiltro(ciudad)}" +
                                            $"   |   Estado: {ValorFiltro(estado)}");
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
                                            .FontColor(
                                                PdfColors.VerdeOscuro);

                                        col.Item()
                                            .Text("Usuario:")
                                            .FontSize(8)
                                            .FontColor(
                                                PdfColors.GrisMedio);

                                        col.Item()
                                            .Text(usuarioGenerador)
                                            .Bold()
                                            .FontSize(9);

                                        col.Item()
                                            .Text("Cargo:")
                                            .FontSize(8)
                                            .FontColor(
                                                PdfColors.GrisMedio);

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
                                "PENDIENTES",
                                totalPendientes.ToString("N0"));

                            Resumen(
                                row,
                                "VERIFICADOS",
                                totalVerificados.ToString("N0"));
                        });

                        content.Item()
                            .Text(
                                $"REGISTROS DE COSTOS DE FLETES ({total})")
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
                                    columns.RelativeColumn(0.7f); // ID
                                    columns.RelativeColumn(1.3f); // Fecha
                                    columns.RelativeColumn(2f);   // Conductor
                                    columns.RelativeColumn(1.2f); // Placa
                                    columns.RelativeColumn(2.2f); // Destino
                                    columns.RelativeColumn(1.2f); // Estado
                                });

                                // CABECERA REPETIDA EN CADA PÁGINA
                                table.Header(header =>
                                {
                                    CeldaCabecera(header, "ID");
                                    CeldaCabecera(header, "Fecha registro");
                                    CeldaCabecera(header, "Conductor");
                                    CeldaCabecera(header, "Placa");
                                    CeldaCabecera(header, "Destino");
                                    CeldaCabecera(header, "Estado");
                                });

                                // FILAS DE LA TABLA
                                foreach (var item in registros)
                                {
                                    var nombreConductor =
                                        item.Autorizacion?.Conductor?.Nombre
                                        ?? item.Trazabilidad?.Conductor
                                        ?? "-";

                                    var placaVehiculo =
                                        item.Autorizacion?.Vehiculo?.Placa
                                        ?? item.Trazabilidad?.Vehiculo
                                        ?? "-";

                                    var destino =
                                        item.Autorizacion?.DestinoCompleto
                                        ?? "-";

                                    CeldaCuerpo(
                                        table,
                                        item.Id.ToString());

                                    CeldaCuerpo(
                                        table,
                                        item.FechaRegistro.ToString(
                                            "dd/MM/yyyy HH:mm"));

                                    CeldaCuerpo(
                                        table,
                                        nombreConductor);

                                    CeldaCuerpo(
                                        table,
                                        placaVehiculo);

                                    CeldaCuerpo(
                                        table,
                                        destino);

                                    CeldaEstado(
                                        table,
                                        item.Estado ?? "-");
                                }
                            });
                        }
                    });

                    // PIE DE PÁGINA CORPORATIVO
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

        private static string FormatoFecha(string? fecha)
        {
            if (string.IsNullOrWhiteSpace(fecha))
                return "No especificada";

            return DateTime.TryParseExact(
                fecha,
                "yyyy-MM-dd",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var resultado)
                ? resultado.ToString("dd/MM/yyyy")
                : fecha;
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
                "verificado" => PdfColors.VerdeOscuro,
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
