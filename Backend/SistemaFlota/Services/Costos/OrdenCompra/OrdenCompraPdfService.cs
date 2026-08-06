using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SistemaFlota.Services.Pdf.Components;
using SistemaFlota.Services.Pdf.Styles;

namespace SistemaFlota.Services.Costos.OrdenCompra
{
    public class OrdenCompraPdfService : IOrdenCompraPdfService
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public OrdenCompraPdfService(
            AppDbContext context,
            IWebHostEnvironment environment)

        {
            _context = context;
            _environment = environment;
        }

        public async Task<byte[]> GenerarPdfAsync(int idOrden)
        {
            var orden = await ObtenerOrden(idOrden);

            var empresa = await _context.ConfiguracionEmpresa
                .FirstOrDefaultAsync();

            if (empresa == null)
                throw new Exception("No existe la configuración de la empresa.");

            var pdf = Document.Create(document =>
            {
                document.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(20);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    ConstruirDocumento(page, orden, empresa);
                });
            });

            return pdf.GeneratePdf();
        }

        private async Task<Models.Costos.OrdenesCompras.OrdenCompra> ObtenerOrden(int id)
        {
            var orden = await _context.OrdenesCompra
                .Include(x => x.Proveedor)
                .Include(x => x.Detalles)
                    .ThenInclude(x => x.Material)
                .Include(x => x.UsuarioCreacion)
                .Include(x => x.UsuarioActualizacion)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (orden == null)
                throw new Exception("La orden de compra no existe.");

            return orden;
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
            Models.Costos.OrdenesCompras.OrdenCompra orden,
            ConfiguracionEmpresa empresa)
        {
            page.Background()
                .Element(container =>
                    MarcaAgua.Dibujar(
                        container, 
                        ObtenerMarcaAgua()));

            page.Header()
                .Element(container =>
                {
                HeaderEmpresa.Dibujar(
                    container,
                    ObtenerLogo(empresa.Logo),
                    empresa,
                    "PEDIDO DE COMPRA",
                    orden.Numero
                    );
                });

            page.Content()
                .PaddingTop(20)
                .Column(col =>
                {
                    col.Item().Row(row =>
                    {
                        // Card Proveedor
                        row.RelativeItem()
                            .Element(container =>
                            {
                                Card.Dibujar(
                                    container,
                                    "Proveedor",
                                    contenido =>
                                    {
                                        contenido.Item().Text($"Nombre: {orden.Proveedor?.Nombre}");
                                        contenido.Item().Text($"NIT: {orden.Proveedor?.Nit}");
                                        contenido.Item().Text($"Contacto: {orden.Proveedor?.Contacto}");
                                        contenido.Item().Text($"Dirección: {orden.Proveedor?.Direccion}");
                                        contenido.Item().Text($"Ciudad: {orden.Proveedor?.Ciudad}");
                                    });
                            });

                        row.ConstantItem(15);

                        // Card Información Orden
                        row.RelativeItem()
                            .Element(container =>
                            {
                                Card.Dibujar(
                                container,
                                "Información de la orden",
                                contenido =>
                                {
                                    contenido.Item().Text($"Fecha: {orden.FechaOrden:dd/MM/yyyy}");
                                    contenido.Item().Text($"Entrega: {(orden.FechaEntrega.HasValue ? orden.FechaEntrega.Value.ToString("dd/MM/yyyy") : "-")}");
                                    contenido.Item().Text($"Forma de pago: {orden.FormaPago}");
                                    contenido.Item().Text($"Lugar de entrega: {orden.LugarEntrega}");

                                    contenido.Item()
                                        .PaddingTop(5)
                                        .Row(row =>
                                        {
                                            row.ConstantItem(60)
                                                .Text("Estado:")
                                                .Style(PdfStyles.Label);

                                            row.AutoItem()
                                               .Element(container =>
                                               {
                                                   EstadoBadge.Dibujar(
                                                       container,
                                                       orden.Estado
                                                   );
                                               });
                                        });
                                });
                            });
                    });

                    col.Item()
                        .PaddingTop(20)
                        .Element(container =>
                        {
                            TablaCorporativa.Dibujar(
                                container,
                                orden.Detalles
                            );
                        });

                    // Totales

                    col.Item()
                        .PaddingTop(40)
                        .Element(container =>
                        {
                            ResumenTotales.Dibujar(
                                container,
                                orden.Subtotal,
                                orden.TipoImpuesto,
                                orden.PorcentajeImpuesto,
                                orden.ValorImpuesto,
                                orden.TotalPagar
                            );
                        });

                    // Observaciones

                    col.Item()
                        .PaddingTop(30)
                        .Element(container =>
                        {
                            Card.Dibujar(
                                container,
                                "OBSERVACIONES",
                                contenido =>
                                {
                                    contenido.Item().Text(
                                        string.IsNullOrWhiteSpace(orden.Observaciones)
                                            ? "Sin observaciones."
                                            : orden.Observaciones
                                    );
                                });
                        });

                    col.Item()
                    .PaddingTop(25)
                    .Element(container =>
                    {
                        Card.Dibujar(
                            container,
                            "TRAZABILIDAD",
                            contenido =>
                            {
                                contenido.Item()
                                    .Text($"Creada por: {orden.UsuarioCreacion?.Username ?? "-"}");

                                contenido.Item()
                                    .Text($"Fecha creación: {orden.FechaCreacion:dd/MM/yyyy HH:mm}");

                                contenido.Item()
                                    .Text($"Actualizada por: {orden.UsuarioActualizacion?.Username ?? "-"}");
                            });
                    });
                });


            page.Footer()
                .Element(FooterEmpresa.Dibujar);
        }

    }
}