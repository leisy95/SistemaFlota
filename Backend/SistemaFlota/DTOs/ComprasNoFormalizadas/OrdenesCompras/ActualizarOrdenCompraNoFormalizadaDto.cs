namespace SistemaFlota.DTOs.ComprasNoFormalizadas.OrdenesCompras
{
    public class ActualizarOrdenCompraNoFormalizadaDto
    {
        public int ProveedorNoFormalizadoId { get; set; }
        public DateTime FechaOrden { get; set; }
        public DateTime? FechaEntrega { get; set; }
        public string FormaPago { get; set; } = string.Empty;
        public string LugarEntrega { get; set; } = string.Empty;
        public string? Observaciones { get; set; }
        public string TipoImpuesto { get; set; } = "IVA";
        public decimal PorcentajeImpuesto { get; set; } = 19;
        public List<CrearOrdenCompraDetalleNoFormalizadaDto> Detalles { get; set; } = new();
    }
}
