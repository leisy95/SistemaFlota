using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SistemaFlota.Services.Pdf.Components;
using SistemaFlota.Services.Pdf.Styles;

namespace SistemaFlota.Services.ComprasNoFormalizadas.OrdenesCompras
{
    public class OrdenCompraNoFormalizadaPdfService
        : IOrdenCompraNoFormalizadaPdfService
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public OrdenCompraNoFormalizadaPdfService(
            AppDbContext context,
            IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        //  GENERAR PDF
        public async Task<byte[]> GenerarPdfAsync(int idOrden)
        {
            var orden = await ObtenerOrden(idOrden);

            var empresa = await _context.ConfiguracionEmpresa
                .FirstOrDefaultAsync();

            if (empresa == null)
                throw new Exception(
                    "No existe la configuración de la empresa.");

            var pdf = Document.Create(document =>
            {
                document.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(20);

                    page.DefaultTextStyle(
                        x => x.FontSize(10));

                    ConstruirDocumento(
                        page,
                        orden,
                        empresa);
                });
            });

            return pdf.GeneratePdf();
        }

        // OBTENER ORDEN

        private async Task<
            Models.ComprasNoFormalizadas.OrdenesCompras
                .OrdenCompraNoFormalizada>
            ObtenerOrden(int id)
        {
            var orden = await _context
                .OrdenesCompraNoFormalizadas

                .Include(x =>
                    x.ProveedorNoFormalizado)

                .Include(x =>
                    x.Detalles)
                    .ThenInclude(x =>
                        x.MaterialNoFormalizado)

                .Include(x =>
                    x.UsuarioCreacion)

                .Include(x =>
                    x.UsuarioActualizacion)

                .FirstOrDefaultAsync(x =>
                    x.Id == id);

            if (orden == null)
                throw new Exception(
                    "La orden de compra no formalizada no existe.");

            return orden;
        }

        // MARCA DE AGUA

        private string ObtenerMarcaAgua()
        {
            return Path.Combine(
                _environment.ContentRootPath,
                "wwwroot",
                "config",
                "iguana3.png"
            );
        }

        // LOGO
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
            Models.ComprasNoFormalizadas.OrdenesCompras
                .OrdenCompraNoFormalizada orden,
            ConfiguracionEmpresa empresa)
        {
            // MARCA DE AGUA
            page.Background()
                .Element(container =>
                    MarcaAgua.Dibujar(
                        container,
                        ObtenerMarcaAgua()));

            // HEADER
            page.Header()
                .Element(container =>
                {
                    HeaderEmpresa.Dibujar(
                        container,
                        ObtenerLogo(empresa.Logo),
                        empresa,
                        "F-GC-027 V2",
                        "",
                        "PEDIDO DE COMPRA",
                        orden.Numero
                    );
                });

            // CONTENIDO
            page.Content()
                .PaddingTop(20)
                .Column(col =>
                {
                    // PROVEEDOR + INFORMACIÓN DE LA ORDEN
                    col.Item()
                        .Row(row =>
                        {
                            // PROVEEDOR
                            row.RelativeItem()
                                .Element(container =>
                                {
                                    Card.Dibujar(
                                        container,
                                        "Proveedor",
                                        contenido =>
                                        {
                                            contenido.Item()
                                                .Text(
                                                    $"Nombre: " +
                                                    $"{orden.ProveedorNoFormalizado?.Nombre ?? "-"}");

                                            contenido.Item()
                                                .Text(
                                                    $"Documento: " +
                                                    $"{orden.ProveedorNoFormalizado?.Documento ?? "-"}");

                                            contenido.Item()
                                                .Text(
                                                    $"Contacto: " +
                                                    $"{orden.ProveedorNoFormalizado?.Contacto ?? "-"}");

                                            contenido.Item()
                                                .Text(
                                                    $"Dirección: " +
                                                    $"{orden.ProveedorNoFormalizado?.Direccion ?? "-"}");

                                            contenido.Item()
                                                .Text(
                                                    $"Ciudad: " +
                                                    $"{orden.ProveedorNoFormalizado?.Ciudad ?? "-"}");
                                        });
                                });

                            row.ConstantItem(15);

                            // INFORMACIÓN DE LA ORDEN

                            row.RelativeItem()
                                .Element(container =>
                                {
                                    Card.Dibujar(
                                        container,
                                        "Información de la orden",
                                        contenido =>
                                        {
                                            contenido.Item()
                                                .Text(
                                                    $"Fecha: " +
                                                    $"{orden.FechaOrden:dd/MM/yyyy}");

                                            contenido.Item()
                                                .Text(
                                                    $"Entrega: " +
                                                    $"{(
                                                        orden.FechaEntrega.HasValue
                                                            ? orden.FechaEntrega.Value
                                                                .ToString("dd/MM/yyyy")
                                                            : "-"
                                                    )}");

                                            contenido.Item()
                                                .Text(
                                                    $"Forma de pago: " +
                                                    $"{orden.FormaPago ?? "-"}");

                                            contenido.Item()
                                                .Text(
                                                    $"Lugar de entrega: " +
                                                    $"{orden.LugarEntrega ?? "-"}");

                                            contenido.Item()
                                                .PaddingTop(5)
                                                .Row(row =>
                                                {
                                                    row.ConstantItem(60)
                                                        .Text("Estado:")
                                                        .Style(
                                                            PdfStyles.Label);

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

                    // DETALLES DE LA ORDEN

                    col.Item()
                        .PaddingTop(20)
                        .Element(container =>
                        {
                            TablaCorporativaNoFormalizada.Dibujar(
                                container,
                                orden.Detalles
                            );
                        });

                    // TOTALES
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

                    // OBSERVACIONES

                    col.Item()
                        .PaddingTop(30)
                        .Element(container =>
                        {
                            Card.Dibujar(
                                container,
                                "OBSERVACIONES",
                                contenido =>
                                {
                                    contenido.Item()
                                        .Text(
                                            string.IsNullOrWhiteSpace(
                                                orden.Observaciones)
                                                ? "Sin observaciones."
                                                : orden.Observaciones
                                        );
                                });
                        });

                    // TRAZABILIDAD
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
                                        .Text(
                                            $"Creada por: " +
                                            $"{orden.UsuarioCreacion?.Username ?? "-"}");

                                    contenido.Item()
                                        .Text(
                                            $"Fecha creación: " +
                                            $"{orden.FechaCreacion:dd/MM/yyyy HH:mm}");

                                    contenido.Item()
                                        .Text(
                                            $"Actualizada por: " +
                                            $"{orden.UsuarioActualizacion?.Username ?? "-"}");

                                    if (orden.FechaActualizacion.HasValue)
                                    {
                                        contenido.Item()
                                            .Text(
                                                $"Fecha actualización: " +
                                                $"{orden.FechaActualizacion.Value:dd/MM/yyyy HH:mm}");
                                    }
                                });
                        });
                });

            // FOOTER
            page.Footer()
                .Element(FooterEmpresa.Dibujar);
        }
    }
}