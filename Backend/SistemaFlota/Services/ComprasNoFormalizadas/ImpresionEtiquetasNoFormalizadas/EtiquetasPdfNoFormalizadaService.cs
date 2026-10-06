using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using SistemaFlota.Services.ComprasNoFormalizadas.ImpresionEtiquetasNoFormalizadas.Componentes;

namespace SistemaFlota.Services.ComprasNoFormalizadas.ImpresionEtiquetasNoFormalizadas
{
    public class EtiquetasPdfNoFormalizadaService
        : IEtiquetasPdfNoFormalizadaService
    {
        private readonly AppDbContext _context;

        public EtiquetasPdfNoFormalizadaService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<byte[]> GenerarAsync(int recepcionId)
        {
            var recepcion = await _context
                .RecepcionesMercanciasNoFormalizadas
                .Include(x => x.Detalles)
                    .ThenInclude(x => x.OrdenCompraDetalleNoFormalizada)
                        .ThenInclude(x => x.MaterialNoFormalizado)
                .Include(x => x.OrdenCompraNoFormalizada)
                    .ThenInclude(x => x.ProveedorNoFormalizado)
                .FirstOrDefaultAsync(x => x.Id == recepcionId);

            if (recepcion == null)
                throw new Exception(
                    "La recepción de mercancía no formalizada no existe.");

            var document = Document.Create(document =>
            {
                foreach (var detalle in recepcion.Detalles)
                {
                    int totalBultos = (int)detalle.BultosRecibidos;

                    string numeroOrden =
                        recepcion
                            .OrdenCompraNoFormalizada!
                            .Numero;

                    string ultimosTresOrden = numeroOrden.Length >= 3
                        ? numeroOrden[^3..]
                        : numeroOrden.PadLeft(3, '0');

                    // Día de la orden: 06
                    string dia =
                        recepcion
                            .OrdenCompraNoFormalizada
                            .FechaOrden
                            .ToString("dd");

                    // Año de la orden: 26
                    string año =
                        recepcion
                            .OrdenCompraNoFormalizada
                            .FechaOrden
                            .ToString("yy");

                    string lote =
                        detalle.LoteProveedor?.Trim() ?? "";

                    string ultimoNumeroLote = "";

                    if (!string.IsNullOrWhiteSpace(lote))
                    {
                        ultimoNumeroLote = lote
                            .Split('-')
                            .Last()
                            .Trim();
                    }

                    // Código interno completo
                    string codigo =
                        $"{ultimosTresOrden}{dia}{año}{ultimoNumeroLote}";

                    // Código que se muestra debajo del QR
                    string codigoFormateado =
                        $"{ultimosTresOrden} · " +
                        $"{dia} · " +
                        $"{año} · " +
                        $"{ultimoNumeroLote}";

                    for (
                        int numeroBulto = 1;
                        numeroBulto <= totalBultos;
                        numeroBulto++)
                    {
                        document.Page(page =>
                        {
                            page.Size(
                                100,
                                50,
                                Unit.Millimetre);

                            page.Margin(
                                2,
                                Unit.Millimetre);

                            page.Content()
                                .Element(container =>
                                {
                                    EtiquetaNoFormalizadaComponent.Dibujar(
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
}