namespace SistemaFlota.DTOs.ComprasNoFormalizadas.RecepcionMercancias
{
    public class RecepcionMercanciaNoFormalizadaDto
    {
        public int Id { get; set; }

        public string ConsecutivoEntrada { get; set; } = string.Empty;

        public int OrdenCompraNoFormalizadaId { get; set; }

        public string NumeroOrden { get; set; } = string.Empty;

        public string Proveedor { get; set; } = string.Empty;

        public DateTime FechaRecepcion { get; set; }

        public string Conductor { get; set; } = string.Empty;

        public string Transportadora { get; set; } = string.Empty;

        public bool EmbalajeAdecuado { get; set; }

        public decimal TotalKg { get; set; }

        public decimal TotalBultos { get; set; }

        public List<RecepcionDetalleConsultaNoFormalizadaDto> Detalles { get; set; }
            = new();
    }
}
