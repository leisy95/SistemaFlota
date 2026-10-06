namespace SistemaFlota.DTOs.ComprasNoFormalizadas.RecepcionMercancias
{
    public class RecepcionDetalleConsultaNoFormalizadaDto
    {
        public string Material { get; set; } = string.Empty;

        public decimal CantidadRecibida { get; set; }

        public decimal BultosRecibidos { get; set; }

        public string LoteProveedor { get; set; } = string.Empty;

        public string EstadoMaterial { get; set; } = string.Empty;

        public string? Observaciones { get; set; }

        public int NumeroEntrega { get; set; }

        public DateTime FechaEntrega { get; set; }

        public bool ProcesadoInventario { get; set; }
    }
}
