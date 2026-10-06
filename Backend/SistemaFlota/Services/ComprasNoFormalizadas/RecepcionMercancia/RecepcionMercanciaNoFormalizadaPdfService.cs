using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using SistemaFlota.Services.Pdf.Components;
using SistemaFlota.Services.Pdf.Styles;

namespace SistemaFlota.Services.ComprasNoFormalizadas.RecepcionMercancia
{
    public class RecepcionMercanciaNoFormalizadaPdfService
        : IRecepcionMercanciaNoFormalizadaPdfService
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public RecepcionMercanciaNoFormalizadaPdfService(
            AppDbContext context,
            IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        public async Task<byte[]> GenerarPdfAsync(int idRecepcion)
        {
            var recepcion =
                await _context
                    .RecepcionesMercanciasNoFormalizadas
                    .AsNoTracking()

                    // ORDEN DE COMPRA + PROVEEDOR
                    .Include(r => r.OrdenCompraNoFormalizada)
                        .ThenInclude(o => o.ProveedorNoFormalizado)

                    // DETALLES + ORDEN DE COMPRA DETALLE + MATERIAL
                    .Include(r => r.Detalles)
                        .ThenInclude(d =>
                            d.OrdenCompraDetalleNoFormalizada)
                        .ThenInclude(od =>
                            od.MaterialNoFormalizado)

                    // USUARIO QUE CONFIRMA
                    .Include(r => r.UsuarioConfirmacion)

                    .FirstOrDefaultAsync(r => r.Id == idRecepcion);

            if (recepcion == null)
            {
                throw new Exception(
                    "La recepción de mercancía no formalizada no existe.");
            }

            var empresa =
                await _context.ConfiguracionEmpresa
                    .AsNoTracking()
                    .FirstOrDefaultAsync();

            if (empresa == null)
            {
                throw new Exception(
                    "No existe la configuración de la empresa.");
            }

            var documento = Document.Create(document =>
            {
                document.Page(page =>
                {
                    page.Size(
                        QuestPDF.Helpers.PageSizes.A4);

                    page.Margin(25);

                    page.DefaultTextStyle(
                        x => x.FontSize(9));

                    ConstruirDocumento(
                        page,
                        recepcion,
                        empresa);
                });
            });

            return documento.GeneratePdf();
        }

        // DOCUMENTO
        private void ConstruirDocumento(
            PageDescriptor page,
            Models.ComprasNoFormalizadas.RecepcionMercancias
                .RecepcionMercanciaNoFormalizada recepcion,
            ConfiguracionEmpresa empresa)
        {
            page.Content()
                .PaddingVertical(12)
                .Column(col =>
                {
                    col.Spacing(10);

                    //  HEADER
                    col.Item()
                        .Element(x =>
                            HeaderEmpresa.Dibujar(
                                x,
                                ObtenerLogo(),
                                empresa,
                                "RECEPCIÓN DE MERCANCÍA NO FORMALIZADA",
                                "F-GC-009 V2",
                                "30/07/2026",
                                recepcion.NumeroRecepcion));

                    // NOTA INICIAL
                    col.Item()
                        .Background(PdfColors.GrisClaro)
                        .Border(1)
                        .BorderColor(PdfColors.GrisClaro)
                        .CornerRadius(5)
                        .Padding(10)
                        .Column(nota =>
                        {
                            nota.Item()
                                .Text("NOTA")
                                .Bold()
                                .FontSize(9)
                                .FontColor(
                                    PdfColors.VerdePrincipal);

                            nota.Item()
                                .PaddingTop(3)
                                .Text(
                                    "Las verificaciones consisten en comprobar la documentacion, condiciones de transporte, caracteristicas físicas de la materia prima, con los criterios de calidad definidos en el procedimiento de recepcion de materias primas.")
                                .FontSize(8)
                                .FontColor(
                                    PdfColors.AzulOscuro);
                        });

                    // DATOS DE RECEPCIÓN + ORDEN
                    col.Item()
                        .Row(row =>
                        {
                            row.RelativeItem()
                                .Element(x =>
                                    DibujarDatosRecepcion(
                                        x,
                                        recepcion));

                            row.ConstantItem(10);

                            row.RelativeItem()
                                .Element(x =>
                                    DibujarDatosOrden(
                                        x,
                                        recepcion));
                        });

                    // TRANSPORTE
                    col.Item()
                        .Element(x =>
                            DibujarTransporte(
                                x,
                                recepcion));

                    // HISTORIAL DE ENTREGAS
                    DibujarEntregas(
                        col,
                        recepcion);

                    // RESUMEN
                    DibujarResumen(
                        col,
                        recepcion);

                    // NOTA FINAL
                    col.Item()
                        .Background(PdfColors.GrisClaro)
                        .Border(1)
                        .BorderColor(PdfColors.GrisClaro)
                        .CornerRadius(5)
                        .Padding(10)
                        .Column(nota =>
                        {
                            nota.Item()
                                .Text("NOTA")
                                .Bold()
                                .FontSize(9)
                                .FontColor(
                                    PdfColors.VerdePrincipal);

                            nota.Item()
                                .PaddingTop(3)
                                .Text(text =>
                                {
                                    text.Span(
                                        "Los métodos de verificación de las características y los criterios de cumplimiento a tener en cuenta, están definidos en el ")
                                        .FontSize(8)
                                        .FontColor(
                                            PdfColors.AzulOscuro);

                                    text.Span(
                                        "\"procedimiento de Pruebas y Ensayos\"")
                                        .Bold()
                                        .FontSize(8)
                                        .FontColor(
                                            PdfColors.AzulOscuro);

                                    text.Span(" e ")
                                        .FontSize(8)
                                        .FontColor(
                                            PdfColors.AzulOscuro);

                                    text.Span(
                                        "\"Instructivos para Pruebas y Ensayos\"")
                                        .Bold()
                                        .FontSize(8)
                                        .FontColor(
                                            PdfColors.AzulOscuro);

                                    text.Span(".")
                                        .FontSize(8)
                                        .FontColor(
                                            PdfColors.AzulOscuro);
                                });
                        });
                });

            page.Footer()
                .Element(FooterEmpresa.Dibujar);
        }

        // DATOS RECEPCIÓN
        private void DibujarDatosRecepcion(
            IContainer container,
            Models.ComprasNoFormalizadas.RecepcionMercancias
                .RecepcionMercanciaNoFormalizada recepcion)
        {
            Card.Dibujar(
                container,
                "DATOS DE LA RECEPCIÓN",
                contenido =>
                {
                    contenido.Item()
                        .Row(row =>
                        {
                            row.RelativeItem()
                                .Column(c =>
                                {
                                    c.Item()
                                        .Text("Número")
                                        .Style(PdfStyles.Label);

                                    c.Item()
                                        .Text(
                                            recepcion.NumeroRecepcion)
                                        .Style(PdfStyles.Valor);
                                });

                            row.RelativeItem()
                                .Column(c =>
                                {
                                    c.Item()
                                        .Text("Fecha")
                                        .Style(PdfStyles.Label);

                                    c.Item()
                                        .Text(
                                            recepcion.FechaRecepcion
                                                .ToString(
                                                    "dd/MM/yyyy HH:mm"))
                                        .Style(PdfStyles.Valor);
                                });
                        });

                    contenido.Item()
                        .PaddingTop(8)
                        .Row(row =>
                        {
                            row.RelativeItem()
                                .Column(c =>
                                {
                                    c.Item()
                                        .Text("Recibe")
                                        .Style(PdfStyles.Label);

                                    c.Item()
                                        .Text(
                                            recepcion.Recibe)
                                        .Style(PdfStyles.Valor);
                                });

                            row.RelativeItem()
                                .Column(c =>
                                {
                                    c.Item()
                                        .Text("Cargo")
                                        .Style(PdfStyles.Label);

                                    c.Item()
                                        .Text(
                                            recepcion.Cargo)
                                        .Style(PdfStyles.Valor);
                                });
                        });
                });
        }

        // ORDEN DE COMPRA
        private void DibujarDatosOrden(
            IContainer container,
            Models.ComprasNoFormalizadas.RecepcionMercancias
                .RecepcionMercanciaNoFormalizada recepcion)
        {
            Card.Dibujar(
                container,
                "ORDEN DE COMPRA",
                contenido =>
                {
                    contenido.Item()
                        .Row(row =>
                        {
                            row.RelativeItem()
                                .Column(c =>
                                {
                                    c.Item()
                                        .Text("Número")
                                        .Style(PdfStyles.Label);

                                    c.Item()
                                        .Text(
                                            recepcion
                                                .OrdenCompraNoFormalizada
                                                ?.Numero ?? "-")
                                        .Style(PdfStyles.Valor);
                                });

                            row.RelativeItem()
                                .Column(c =>
                                {
                                    c.Item()
                                        .Text("Proveedor")
                                        .Style(PdfStyles.Label);

                                    c.Item()
                                        .Text(
                                            recepcion
                                                .OrdenCompraNoFormalizada
                                                ?.ProveedorNoFormalizado
                                                ?.Nombre ?? "-")
                                        .Style(PdfStyles.Valor);
                                });
                        });

                    contenido.Item()
                        .PaddingTop(8)
                        .Row(row =>
                        {
                            row.RelativeItem()
                                .Column(c =>
                                {
                                    c.Item()
                                        .Text("Fecha")
                                        .Style(PdfStyles.Label);

                                    c.Item()
                                        .Text(
                                            recepcion
                                                .OrdenCompraNoFormalizada
                                                ?.FechaOrden
                                                .ToString(
                                                    "dd/MM/yyyy")
                                            ?? "-")
                                        .Style(PdfStyles.Valor);
                                });

                            row.RelativeItem()
                                .Column(c =>
                                {
                                    c.Item()
                                        .Text("Entrega")
                                        .Style(PdfStyles.Label);

                                    c.Item()
                                        .Text(
                                            recepcion
                                                .OrdenCompraNoFormalizada
                                                ?.FechaEntrega?
                                                .ToString(
                                                    "dd/MM/yyyy")
                                            ?? "-")
                                        .Style(PdfStyles.Valor);
                                });
                        });

                    contenido.Item()
                        .PaddingTop(8)
                        .Column(c =>
                        {
                            c.Item()
                                .Text("Estado")
                                .Style(PdfStyles.Label);

                            c.Item()
                                .Text(
                                    recepcion
                                        .OrdenCompraNoFormalizada
                                        ?.Estado ?? "-")
                                .Style(PdfStyles.Valor);
                        });
                });
        }

        // TRANSPORTE
        private void DibujarTransporte(
            IContainer container,
            Models.ComprasNoFormalizadas.RecepcionMercancias
                .RecepcionMercanciaNoFormalizada recepcion)
        {
            Card.Dibujar(
                container,
                "TRANSPORTE",
                contenido =>
                {
                    contenido.Item()
                        .Row(row =>
                        {
                            row.RelativeItem()
                                .Column(c =>
                                {
                                    c.Item()
                                        .Text("Conductor")
                                        .Style(PdfStyles.Label);

                                    c.Item()
                                        .Text(
                                            recepcion.Conductor)
                                        .Style(PdfStyles.Valor);
                                });

                            row.RelativeItem()
                                .Column(c =>
                                {
                                    c.Item()
                                        .Text("Transportadora")
                                        .Style(PdfStyles.Label);

                                    c.Item()
                                        .Text(
                                            recepcion.Transportadora)
                                        .Style(PdfStyles.Valor);
                                });
                        });

                    contenido.Item()
                        .PaddingTop(8)
                        .Row(row =>
                        {
                            row.RelativeItem()
                                .Column(c =>
                                {
                                    c.Item()
                                        .Text("Tipo documento")
                                        .Style(PdfStyles.Label);

                                    c.Item()
                                        .Text(
                                            recepcion.TipoDocumento)
                                        .Style(PdfStyles.Valor);
                                });

                            row.RelativeItem()
                                .Column(c =>
                                {
                                    c.Item()
                                        .Text("Embalaje adecuado")
                                        .Style(PdfStyles.Label);

                                    c.Item()
                                        .Text(
                                            recepcion.EmbalajeAdecuado
                                                ? "Sí"
                                                : "No")
                                        .Style(PdfStyles.Valor);
                                });
                        });
                });
        }

        // HISTORIAL DE ENTREGAS
        private void DibujarEntregas(
    ColumnDescriptor col,
    Models.ComprasNoFormalizadas.RecepcionMercancias
        .RecepcionMercanciaNoFormalizada recepcion)
        {
            var entregas = recepcion.Detalles
                .GroupBy(d => d.NumeroEntrega)
                .OrderBy(g => g.Key)
                .ToList();

            col.Item()
                .Text("HISTORIAL DE ENTREGAS")
                .Style(PdfStyles.Subtitulo);

            if (!entregas.Any())
            {
                col.Item()
                    .Text("No existen entregas registradas.")
                    .Style(PdfStyles.Valor);

                return;
            }

            foreach (var entrega in entregas)
            {
                var fechaEntrega = entrega
                    .Min(x => x.FechaEntrega);

                var totalKg = entrega
                    .Sum(x => x.CantidadRecibida);

                var totalBultos = entrega
                    .Sum(x => x.BultosRecibidos);

                var procesada = entrega
                    .All(x => x.ProcesadoInventario);

                col.Item()
                    .Element(x =>
                    {
                        Card.Dibujar(
                            x,
                            $"ENTREGA #{entrega.Key}",
                            contenido =>
                            {
                                // INFORMACIÓN DE LA ENTREGA
                                contenido.Item()
                                    .Row(row =>
                                    {
                                        row.RelativeItem()
                                            .Column(c =>
                                            {
                                                c.Item()
                                                    .Text("Fecha de entrega")
                                                    .Style(PdfStyles.Label);

                                                c.Item()
                                                    .Text(
                                                        fechaEntrega.ToString(
                                                            "dd/MM/yyyy HH:mm"))
                                                    .Style(PdfStyles.Valor);
                                            });

                                        row.RelativeItem()
                                            .Column(c =>
                                            {
                                                c.Item()
                                                    .Text("Estado")
                                                    .Style(PdfStyles.Label);

                                                c.Item()
                                                    .Text(
                                                        procesada
                                                            ? "Ingresada a inventario"
                                                            : "Pendiente de inventario")
                                                    .Style(PdfStyles.Valor);
                                            });
                                    });

                                // TABLA DE LA ENTREGA
                                contenido.Item()
                                    .PaddingTop(10)
                                    .Element(tabla =>
                                        TablaRecepcionMercanciaNoFormalizada.Dibujar(
                                            tabla,
                                            entrega.ToList()));

                                // TOTALES DE LA ENTREGA
                                contenido.Item()
                                    .PaddingTop(8)
                                    .Row(row =>
                                    {
                                        row.RelativeItem()
                                            .Text(
                                                $"Total KG: {totalKg:N2}")
                                            .Style(PdfStyles.Valor);

                                        row.RelativeItem()
                                            .Text(
                                                $"Total Bultos: {totalBultos:N2}")
                                            .Style(PdfStyles.Valor);
                                    });
                            });
                    });
            }
        }

        // RESUMEN
        private void DibujarResumen(
            ColumnDescriptor col,
            Models.ComprasNoFormalizadas.RecepcionMercancias
                .RecepcionMercanciaNoFormalizada recepcion)
        {
            var totalKg =
                recepcion.Detalles
                    .Sum(x => x.CantidadRecibida);

            var totalBultos =
                recepcion.Detalles
                    .Sum(x => x.BultosRecibidos);

            var totalEntregas =
                recepcion.Detalles
                    .Select(x => x.NumeroEntrega)
                    .Distinct()
                    .Count();

            col.Item()
                .Element(x =>
                {
                    Card.Dibujar(
                        x,
                        "RESUMEN DE LA RECEPCIÓN",
                        contenido =>
                        {
                            // TOTALES
                            contenido.Item()
                                .Row(row =>
                                {
                                    row.RelativeItem()
                                        .Column(c =>
                                        {
                                            c.Item()
                                                .Text("Entregas")
                                                .Style(
                                                    PdfStyles.Label);

                                            c.Item()
                                                .Text(
                                                    totalEntregas
                                                        .ToString())
                                                .Style(
                                                    PdfStyles.Total);
                                        });

                                    row.RelativeItem()
                                        .Column(c =>
                                        {
                                            c.Item()
                                                .Text("Total KG")
                                                .Style(
                                                    PdfStyles.Label);

                                            c.Item()
                                                .Text(
                                                    totalKg
                                                        .ToString("N2"))
                                                .Style(
                                                    PdfStyles.Total);
                                        });

                                    row.RelativeItem()
                                        .Column(c =>
                                        {
                                            c.Item()
                                                .Text("Total Bultos")
                                                .Style(
                                                    PdfStyles.Label);

                                            c.Item()
                                                .Text(
                                                    totalBultos
                                                        .ToString("N2"))
                                                .Style(
                                                    PdfStyles.Total);
                                        });
                                });

                            // ESTADO
                            contenido.Item()
                                .PaddingTop(15)
                                .Text(
                                    "ESTADO DE LA RECEPCIÓN")
                                .Bold()
                                .FontSize(11);

                            contenido.Item()
                                .PaddingTop(5)
                                .PaddingLeft(15)
                                .Column(c =>
                                {
                                    bool confirmada =
                                        recepcion
                                            .FechaConfirmacion
                                            .HasValue;

                                    c.Item()
                                        .Text(
                                            $"Estado: {(confirmada
                                                ? "Confirmada"
                                                : "Pendiente")}");

                                    c.Item()
                                        .Text(
                                            $"Usuario: {recepcion
                                                .UsuarioConfirmacion
                                                ?.Username ?? "---"}");

                                    c.Item()
                                        .Text(
                                            $"Fecha: {(recepcion
                                                .FechaConfirmacion
                                                .HasValue
                                                ? recepcion
                                                    .FechaConfirmacion
                                                    .Value
                                                    .ToString(
                                                        "dd/MM/yyyy HH:mm")
                                                : "---")}");
                                });
                        });
                });
        }

        // LOGO

        private string ObtenerLogo()
        {
            return Path.Combine(
                _environment.ContentRootPath,
                "wwwroot",
                "config",
                "logo.png");
        }
    }
}