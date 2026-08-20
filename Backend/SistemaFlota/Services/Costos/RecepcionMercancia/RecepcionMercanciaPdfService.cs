using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using SistemaFlota.Services.Pdf.Components;

namespace SistemaFlota.Services.Pdf.RecepcionMercancia;

public class RecepcionMercanciaPdfService : IRecepcionMercanciaPdfService
{
    private readonly AppDbContext _context;
    private readonly IWebHostEnvironment _environment;

    public RecepcionMercanciaPdfService(
        AppDbContext context,
        IWebHostEnvironment environment)
    {
        _context = context;
        _environment = environment;
    }

    public async Task<byte[]> GenerarPdfAsync(int idRecepcion)
    {
        var recepcion = await _context.RecepcionesMercancias
            .AsNoTracking()
            .Include(r => r.OrdenCompra)
                .ThenInclude(o => o.Proveedor)
            .Include(r => r.Detalles)
                .ThenInclude(d => d.OrdenCompraDetalle)
                    .ThenInclude(od => od.Material)
            .FirstOrDefaultAsync(r => r.Id == idRecepcion);

        if (recepcion == null)
            throw new Exception("La recepción de mercancía no existe.");

        var empresa = await _context.ConfiguracionEmpresa
            .AsNoTracking()
            .FirstOrDefaultAsync();

        if (empresa == null)
            throw new Exception("No existe la configuración de la empresa.");

        var documento = Document.Create(document =>
        {
            document.Page(page =>
            {
                page.Size(QuestPDF.Helpers.PageSizes.A4);
                page.Margin(20);

                page.DefaultTextStyle(x => x.FontSize(10));

                ConstruirDocumento(
                    page,
                    recepcion,
                    empresa
                );
            });
        });

        return documento.GeneratePdf();
    }

    private void ConstruirDocumento(
    PageDescriptor page,
    Models.Costos.RecepcionMercancias.RecepcionMercancia recepcion,
    ConfiguracionEmpresa empresa)
    {
        page.Header()
            .Element(x =>
            {
                HeaderEmpresa.Dibujar(
                    x,
                    ObtenerLogo(),
                    empresa,
                    "RECEPCIÓN DE MERCANCÍA",
                    recepcion.NumeroRecepcion);
            });

        page.Content()
            .PaddingVertical(10)
            .Column(col =>
            {
                col.Spacing(7);

                DibujarDatosRecepcion(col, recepcion);
                DibujarDatosOrden(col, recepcion);
                DibujarTransporte(col, recepcion);

                col.Item().Element(x =>
                {
                    TablaRecepcionMercancia.Dibujar(
                        x,
                        recepcion.Detalles);
                });

                DibujarResumen(col, recepcion);
            });

        page.Footer()
            .Element(FooterEmpresa.Dibujar);
    }

    private void DibujarDatosRecepcion(
    ColumnDescriptor col,
    Models.Costos.RecepcionMercancias.RecepcionMercancia recepcion)
    {
        col.Item().Element(x =>
        {
            Card.Dibujar(x, "DATOS DE LA RECEPCIÓN", contenido =>
            {
                contenido.Item().Row(row =>
                {
                    row.RelativeItem().Text(
                        $"Recepción: {recepcion.NumeroRecepcion}");

                    row.RelativeItem().Text(
                        $"Fecha: {recepcion.FechaRecepcion:dd/MM/yyyy}");

                    row.RelativeItem().Text(
                        $"Recibe: {recepcion.Recibe}");

                    row.RelativeItem().Text(
                        $"Cargo: {recepcion.Cargo}");
                });
            });
        });
    }

    private void DibujarDatosOrden(
    ColumnDescriptor col,
    Models.Costos.RecepcionMercancias.RecepcionMercancia recepcion)
    {
        col.Item().Element(x =>
        {
            Card.Dibujar(x, "ORDEN DE COMPRA", contenido =>
            {
                contenido.Item().Row(row =>
                {
                    row.RelativeItem().Text(
                        $"Orden: {recepcion.OrdenCompra?.Numero}");

                    row.RelativeItem().Text(
                        $"Proveedor: {recepcion.OrdenCompra?.Proveedor?.Nombre}");

                    row.RelativeItem().Text(
                        $"Fecha: {recepcion.OrdenCompra?.FechaOrden:dd/MM/yyyy}");

                    row.RelativeItem().Text(
                        $"Estado: {recepcion.OrdenCompra?.Estado}");
                });

                if (recepcion.OrdenCompra?.FechaEntrega != null)
                {
                    contenido.Item()
                        .PaddingTop(3)
                        .Text(
                            $"Fecha de entrega: {recepcion.OrdenCompra.FechaEntrega:dd/MM/yyyy}");
                }
            });
        });
    }

    private void DibujarTransporte(
    ColumnDescriptor col,
    Models.Costos.RecepcionMercancias.RecepcionMercancia recepcion)
    {
        col.Item().Element(x =>
        {
            Card.Dibujar(x, "TRANSPORTE", contenido =>
            {
                contenido.Item().Row(row =>
                {
                    row.RelativeItem().Text(
                        $"Conductor: {recepcion.Conductor}");

                    row.RelativeItem().Text(
                        $"Transportadora: {recepcion.Transportadora}");

                    row.RelativeItem().Text(
                        $"Documento: {recepcion.TipoDocumento}");

                    row.RelativeItem().Text(
                        $"Embalaje: {(recepcion.EmbalajeAdecuado ? "Sí" : "No")}");
                });
            });
        });
    }

    private void DibujarResumen(
    ColumnDescriptor col,
    Models.Costos.RecepcionMercancias.RecepcionMercancia recepcion)
    {
        col.Item().Element(x =>
        {
            Card.Dibujar(x, "RESUMEN DE LA RECEPCIÓN", contenido =>
            {
                contenido.Item().Row(row =>
                {
                    row.RelativeItem().Text(
                        $"Materiales: {recepcion.OrdenCompra?.TotalItems}");

                    row.RelativeItem().Text(
                        $"Total Kg: {recepcion.OrdenCompra?.TotalKg:N2}");

                    row.RelativeItem().Text(
                        $"Bultos: {recepcion.OrdenCompra?.TotalBultos:N2}");

                    row.RelativeItem().Text(
                        $"Subtotal: ${recepcion.OrdenCompra?.Subtotal:N2}");

                    row.RelativeItem().Text(
                        $"Impuesto: ${recepcion.OrdenCompra?.ValorImpuesto:N2}");

                    row.RelativeItem().Text(
                        $"Total: ${recepcion.OrdenCompra?.TotalPagar:N2}");
                });
            });
        });
    }

    private void DibujarEstadoProceso(
    ColumnDescriptor col,
    Models.Costos.RecepcionMercancias.RecepcionMercancia recepcion)
    {
        col.Item().Element(x =>
        {
            Card.Dibujar(x, "ESTADO DEL PROCESO", contenido =>
            {
                // Etapa 1
                contenido.Item().Text("ORDEN CREADA").Bold().FontSize(11);

                contenido.Item().PaddingLeft(15).Column(c =>
                {
                    c.Item().Text($"Orden: {recepcion.OrdenCompra?.Numero}");
                    c.Item().Text($"Fecha: {recepcion.OrdenCompra?.FechaCreacion:dd/MM/yyyy HH:mm}");
                    c.Item().Text($"Estado: {recepcion.OrdenCompra?.Estado}");
                });

                contenido.Item().PaddingTop(10);

                // Etapa 2
                contenido.Item().Text("MERCANCÍA RECIBIDA").Bold().FontSize(11);

                contenido.Item().PaddingLeft(15).Column(c =>
                {
                    c.Item().Text($"Recibe: {recepcion.Recibe}");
                    c.Item().Text($"Cargo: {recepcion.Cargo}");
                    c.Item().Text($"Fecha: {recepcion.FechaRecepcion:dd/MM/yyyy HH:mm}");
                });

                contenido.Item().PaddingTop(10);

                // Etapa 3
                contenido.Item().Text("RECEPCIÓN CONFIRMADA").Bold().FontSize(11);

                contenido.Item().PaddingLeft(15).Column(c =>
                {
                    c.Item().Text("Estado: Pendiente");
                    c.Item().Text("Usuario: ---");
                    c.Item().Text("Fecha: ---");
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