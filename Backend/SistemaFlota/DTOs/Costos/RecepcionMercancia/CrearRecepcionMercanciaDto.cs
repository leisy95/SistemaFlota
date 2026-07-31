namespace SistemaFlota.DTOs.Costos.RecepcionMercancia
{
    public class CrearRecepcionMercanciaDto
    {
        public int OrdenCompraId { get; set; }

        public string ConsecutivoEntrada { get; set; } = string.Empty;

        public string Conductor { get; set; } = string.Empty;

        public string Transportadora { get; set; } = string.Empty;

        public string TipoDocumento { get; set; } = string.Empty;

        public bool EmbalajeAdecuado { get; set; }

        public string Recibe { get; set; } = string.Empty;

        public string Cargo { get; set; } = string.Empty;

        public string? Observaciones { get; set; }

        public List<RecepcionMercanciaDetalleDto> Detalles { get; set; }
            = new();
    }
}