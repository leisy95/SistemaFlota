namespace SistemaFlota.DTOs.ComprasNoFormalizadas.RecepcionMercancias
{
    public class CrearRecepcionMercanciaNoFormalizadaDto
    {
        public int OrdenCompraNoFormalizadaId { get; set; }

        public string ConsecutivoEntrada { get; set; } = string.Empty;

        public string Conductor { get; set; } = string.Empty;

        public string Transportadora { get; set; } = string.Empty;

        public string TipoDocumento { get; set; } = string.Empty;

        public bool EmbalajeAdecuado { get; set; }

        public string Recibe { get; set; } = string.Empty;

        public string Cargo { get; set; } = string.Empty;

        public string? Observaciones { get; set; }

        public List<int> Usuarios { get; set; } = new();

        public List<RecepcionMercanciaDetalleNoFormalizadaDto> Detalles { get; set; }
            = new();
    }
}
