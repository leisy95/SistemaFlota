using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SistemaFlota;
using SistemaFlota.Services.Pdf.EtiquetasQR.Components;

namespace SistemaFlota.Services.ImpresionEtiquetas;

public class EtiquetasPdfService : IEtiquetasPdfService
{
    private readonly AppDbContext _context;

    public EtiquetasPdfService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<byte[]> GenerarAsync(int recepcionId)
    {
        // 1. Cargar la recepción actual con sus detalles completos
        var recepcion = await _context.RecepcionesMercancias
            .AsNoTracking()
            .Include(x => x.Detalles)
                .ThenInclude(x => x.OrdenCompraDetalle)
                    .ThenInclude(x => x.Material)
            .Include(x => x.OrdenCompra)
                .ThenInclude(x => x.Proveedor)
            .FirstAsync(x => x.Id == recepcionId);

        // 2. Determinar el número de la última entrega realizada en este registro
        int ultimaEntregaActual = recepcion.Detalles.Any()
            ? recepcion.Detalles.Max(x => x.NumeroEntrega)
            : 1;

        // 3. Filtrar los detalles recibidos en esta entrega específica
        var detallesEntregaActual = recepcion.Detalles
            .Where(x => x.NumeroEntrega == ultimaEntregaActual)
            .ToList();

        // 4. Obtener todos los detalles de entregas ANTERIORES
        //    para la misma Orden de Compra
        var detallesAnteriores = await _context.RecepcionesMercancias
            .AsNoTracking()
            .Where(x => x.OrdenCompraId == recepcion.OrdenCompraId)
            .SelectMany(x => x.Detalles)
            .Where(x => x.NumeroEntrega < ultimaEntregaActual)
            .ToListAsync();

        var document = Document.Create(document =>
        {
            foreach (var detalle in detallesEntregaActual)
            {
                int bultosActuales = (int)detalle.BultosRecibidos;
                int ordenCompraDetalleId = detalle.OrdenCompraDetalleId;

                // Sumar bultos ingresados en entregas anteriores
                int bultosAnteriores = detallesAnteriores
                    .Where(x => x.OrdenCompraDetalleId == ordenCompraDetalleId)
                    .Sum(x => (int)x.BultosRecibidos);

                // Total de bultos programados en la Orden de Compra
                int totalBultos = (int)detalle.OrdenCompraDetalle!.Bultos;

                // CÓDIGO DE TRAZABILIDAD

                string numeroOrden = recepcion.OrdenCompra!.Numero;

                // Últimos 3 números de la orden
                string ultimosTresOrden = numeroOrden.Length >= 3
                    ? numeroOrden[^3..]
                    : numeroOrden.PadLeft(3, '0');

                // Día de la fecha
                string dia = recepcion.OrdenCompra.FechaOrden
                    .ToString("dd");

                // Últimos 2 dígitos del año
                string año = recepcion.OrdenCompra.FechaOrden
                    .ToString("yy");

                // Lote completo
                string lote = detalle.LoteProveedor?.Trim() ?? "";

                // Parte del lote después del último "-"
                string ultimoNumeroLote = "";

                if (!string.IsNullOrWhiteSpace(lote))
                {
                    ultimoNumeroLote = lote
                        .Split('-')
                        .Last()
                        .Trim();
                }


                string codigoFormateado =
                    $"{ultimosTresOrden} · {dia} · {año} · {ultimoNumeroLote}";

                // GENERAR PÁGINAS CONSECUTIVAS

                for (int i = 1; i <= bultosActuales; i++)
                {
                    int numeroBulto = bultosAnteriores + i;

                    document.Page(page =>
                    {
                        page.Size(100, 50, Unit.Millimetre);
                        page.Margin(2, Unit.Millimetre);

                        page.Content().Element(container =>
                        {
                            EtiquetaComponent.Dibujar(
                                container,
                                recepcion,
                                detalle,
                                numeroBulto,
                                totalBultos,
                                codigoFormateado);
                        });
                    });
                }
            }
        });

        return document.GeneratePdf();
    }
}