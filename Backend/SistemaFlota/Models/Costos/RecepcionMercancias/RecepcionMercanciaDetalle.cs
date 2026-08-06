using SistemaFlota.Models.Costos.OrdenesCompras;

namespace SistemaFlota.Models.Costos.RecepcionMercancias
{
    public class RecepcionMercanciaDetalle
    {
        public int Id { get; set; }

        public int RecepcionMercanciaId { get; set; }

        public RecepcionMercancia? RecepcionMercancia { get; set; }

        public int OrdenCompraDetalleId { get; set; }

        public OrdenCompraDetalle? OrdenCompraDetalle { get; set; }

        public decimal CantidadRecibida { get; set; }

        public decimal BultosRecibidos { get; set; }

        public string LoteProveedor { get; set; } = string.Empty;

        public string EstadoMaterial { get; set; } = string.Empty;

        public string? Observaciones { get; set; }
    }
}