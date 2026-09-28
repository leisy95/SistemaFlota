using SistemaFlota.Models.ComprasNoFormalizadas.OrdenesCompras;

namespace SistemaFlota.Models.ComprasNoFormalizadas.RecepcionMercancias
{
    public class RecepcionMercanciaDetalleNoFormalizada
    {
        public int Id { get; set; }

        public int RecepcionMercanciaNoFormalizadaId { get; set; }

        public RecepcionMercanciaNoFormalizada? RecepcionMercanciaNoFormalizada { get; set; }

        public int OrdenCompraDetalleNoFormalizadaId { get; set; }

        public OrdenCompraDetalleNoFormalizada? OrdenCompraDetalleNoFormalizada { get; set; }

        public decimal CantidadRecibida { get; set; }

        public decimal BultosRecibidos { get; set; }

        public string LoteProveedor { get; set; } = string.Empty;

        public string EstadoMaterial { get; set; } = string.Empty;

        public string? Observaciones { get; set; }
    }
}
