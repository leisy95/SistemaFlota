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
        var recepcion = await _context.RecepcionesMercancia
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
    Models.Costos.RecepcionMercancia.RecepcionMercancia recepcion,
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
            .PaddingVertical(20)
            .Column(col =>
            {
                col.Spacing(15);

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
    Models.Costos.RecepcionMercancia.RecepcionMercancia recepcion)
    {
        // Card con número de recepción, fecha, recibe, cargo...
    }

    private void DibujarDatosOrden(
        ColumnDescriptor col,
        Models.Costos.RecepcionMercancia.RecepcionMercancia recepcion)
    {
        // Card con orden de compra, proveedor...
    }

    private void DibujarTransporte(
        ColumnDescriptor col,
        Models.Costos.RecepcionMercancia.RecepcionMercancia recepcion)
    {
        // Card con conductor, transportadora, tipo documento...
    }

    private void DibujarResumen(
        ColumnDescriptor col,
        Models.Costos.RecepcionMercancia.RecepcionMercancia recepcion)
    {
        // Totales recibidos
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