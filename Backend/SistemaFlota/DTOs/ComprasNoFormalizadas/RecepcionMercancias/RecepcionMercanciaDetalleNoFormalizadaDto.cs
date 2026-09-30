namespace SistemaFlota.DTOs.ComprasNoFormalizadas.RecepcionMercancias
{
    public class RecepcionMercanciaDetalleNoFormalizadaDto
    {
        public int OrdenCompraDetalleNoFormalizadaId { get; set; }

        public decimal CantidadRecibida { get; set; }

        public decimal BultosRecibidos { get; set; }

        public string LoteProveedor { get; set; } = string.Empty;

        public string EstadoMaterial { get; set; } = string.Empty;

        public string? Observaciones { get; set; }
    }
}
