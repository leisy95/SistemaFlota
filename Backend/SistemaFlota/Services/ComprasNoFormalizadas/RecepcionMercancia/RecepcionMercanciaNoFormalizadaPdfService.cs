using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using SistemaFlota.Services.Pdf.Components;

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
            var recepcion = await _context
                .RecepcionesMercanciasNoFormalizadas
                .AsNoTracking()
                .Include(r => r.OrdenCompraNoFormalizada)
                    .ThenInclude(o => o.ProveedorNoFormalizado)
                .Include(r => r.Detalles)
                    .ThenInclude(d => d.OrdenCompraDetalleNoFormalizada)
                        .ThenInclude(od => od.MaterialNoFormalizado)
                .FirstOrDefaultAsync(r => r.Id == idRecepcion);

            if (recepcion == null)
                throw new Exception(
                    "La recepción de mercancía no formalizada no existe.");

            var empresa = await _context.ConfiguracionEmpresa
                .AsNoTracking()
                .FirstOrDefaultAsync();

            if (empresa == null)
                throw new Exception(
                    "No existe la configuración de la empresa.");

            var documento = Document.Create(document =>
            {
                document.Page(page =>
                {
                    page.Size(QuestPDF.Helpers.PageSizes.A4);
                    page.Margin(20);

                    page.DefaultTextStyle(
                        x => x.FontSize(10));

                    ConstruirDocumento(
                        page,
                        recepcion,
                        empresa);
                });
            });

            return documento.GeneratePdf();
        }

        private void ConstruirDocumento(
            PageDescriptor page,
            Models.ComprasNoFormalizadas.RecepcionMercancias.RecepcionMercanciaNoFormalizada recepcion,
            ConfiguracionEmpresa empresa)
        {
            page.Header()
                .Element(x =>
                {
                    HeaderEmpresa.Dibujar(
                        x,
                        ObtenerLogo(),
                        empresa,
                        "",
                        "RECEPCIÓN DE MERCANCÍA NO FORMALIZADA",
                        recepcion.NumeroRecepcion);
                });

            page.Content()
                .PaddingVertical(10)
                .Column(col =>
                {
                    col.Spacing(7);

                    DibujarDatosRecepcion(
                        col,
                        recepcion);

                    DibujarDatosOrden(
                        col,
                        recepcion);

                    DibujarTransporte(
                        col,
                        recepcion);

                    col.Item()
                        .Element(x =>
                        {
                            TablaRecepcionMercanciaNoFormalizada.Dibujar(
                                x,
                                recepcion.Detalles);
                        });

                    DibujarResumen(
                        col,
                        recepcion);
                });

            page.Footer()
                .Element(FooterEmpresa.Dibujar);
        }

        private void DibujarDatosRecepcion(
            ColumnDescriptor col,
            Models.ComprasNoFormalizadas.RecepcionMercancias.RecepcionMercanciaNoFormalizada recepcion)
        {
            col.Item()
                .Element(x =>
                {
                    Card.Dibujar(
                        x,
                        "DATOS DE LA RECEPCIÓN",
                        contenido =>
                        {
                            contenido.Item()
                                .Row(row =>
                                {
                                    row.RelativeItem()
                                        .Text(
                                            $"Recepción: {recepcion.NumeroRecepcion}");

                                    row.RelativeItem()
                                        .Text(
                                            $"Fecha: {recepcion.FechaRecepcion:dd/MM/yyyy}");

                                    row.RelativeItem()
                                        .Text(
                                            $"Recibe: {recepcion.Recibe}");

                                    row.RelativeItem()
                                        .Text(
                                            $"Cargo: {recepcion.Cargo}");
                                });
                        });
                });
        }

        private void DibujarDatosOrden(
            ColumnDescriptor col,
            Models.ComprasNoFormalizadas.RecepcionMercancias.RecepcionMercanciaNoFormalizada recepcion)
        {
            col.Item()
                .Element(x =>
                {
                    Card.Dibujar(
                        x,
                        "ORDEN DE COMPRA",
                        contenido =>
                        {
                            contenido.Item()
                                .Row(row =>
                                {
                                    row.RelativeItem()
                                        .Text(
                                            $"Orden: {recepcion.OrdenCompraNoFormalizada?.Numero}");

                                    row.RelativeItem()
                                        .Text(
                                            $"Proveedor: {recepcion.OrdenCompraNoFormalizada?.ProveedorNoFormalizado?.Nombre}");

                                    row.RelativeItem()
                                        .Text(
                                            $"Fecha: {recepcion.OrdenCompraNoFormalizada?.FechaOrden:dd/MM/yyyy}");

                                    row.RelativeItem()
                                        .Text(
                                            $"Estado: {recepcion.OrdenCompraNoFormalizada?.Estado}");
                                });

                            if (recepcion
                                .OrdenCompraNoFormalizada
                                ?.FechaEntrega != null)
                            {
                                contenido.Item()
                                    .PaddingTop(3)
                                    .Text(
                                        $"Fecha de entrega: {recepcion.OrdenCompraNoFormalizada.FechaEntrega:dd/MM/yyyy}");
                            }
                        });
                });
        }

        private void DibujarTransporte(
            ColumnDescriptor col,
            Models.ComprasNoFormalizadas.RecepcionMercancias.RecepcionMercanciaNoFormalizada recepcion)
        {
            col.Item()
                .Element(x =>
                {
                    Card.Dibujar(
                        x,
                        "TRANSPORTE",
                        contenido =>
                        {
                            contenido.Item()
                                .Row(row =>
                                {
                                    row.RelativeItem()
                                        .Text(
                                            $"Conductor: {recepcion.Conductor}");

                                    row.RelativeItem()
                                        .Text(
                                            $"Transportadora: {recepcion.Transportadora}");

                                    row.RelativeItem()
                                        .Text(
                                            $"Documento: {recepcion.TipoDocumento}");

                                    row.RelativeItem()
                                        .Text(
                                            $"Embalaje: {(recepcion.EmbalajeAdecuado ? "Sí" : "No")}");
                                });
                        });
                });
        }

        private void DibujarResumen(
            ColumnDescriptor col,
            Models.ComprasNoFormalizadas.RecepcionMercancias.RecepcionMercanciaNoFormalizada recepcion)
        {
            col.Item()
                .Element(x =>
                {
                    Card.Dibujar(
                        x,
                        "RESUMEN DE LA RECEPCIÓN",
                        contenido =>
                        {
                            contenido.Item()
                                .Row(row =>
                                {
                                    row.RelativeItem()
                                        .Text(
                                            $"Materiales: {recepcion.OrdenCompraNoFormalizada?.TotalItems}");

                                    row.RelativeItem()
                                        .Text(
                                            $"Total Kg: {recepcion.OrdenCompraNoFormalizada?.TotalKg:N2}");

                                    row.RelativeItem()
                                        .Text(
                                            $"Bultos: {recepcion.OrdenCompraNoFormalizada?.TotalBultos:N2}");

                                    row.RelativeItem()
                                        .Text(
                                            $"Subtotal: ${recepcion.OrdenCompraNoFormalizada?.Subtotal:N2}");

                                    row.RelativeItem()
                                        .Text(
                                            $"Impuesto: ${recepcion.OrdenCompraNoFormalizada?.ValorImpuesto:N2}");

                                    row.RelativeItem()
                                        .Text(
                                            $"Total: ${recepcion.OrdenCompraNoFormalizada?.TotalPagar:N2}");
                                });
                        });
                });
        }

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