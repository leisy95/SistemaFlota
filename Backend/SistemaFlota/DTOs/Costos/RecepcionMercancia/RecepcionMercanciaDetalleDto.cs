namespace SistemaFlota.DTOs.Costos.RecepcionMercancia
{
    public class RecepcionMercanciaDetalleDto
    {
        public int OrdenCompraDetalleId { get; set; }

        public decimal CantidadRecibida { get; set; }

        public decimal BultosRecibidos { get; set; }

        public string LoteProveedor { get; set; } = string.Empty;

        public string EstadoMaterial { get; set; } = string.Empty;

        public string? Observaciones { get; set; }
    }
}