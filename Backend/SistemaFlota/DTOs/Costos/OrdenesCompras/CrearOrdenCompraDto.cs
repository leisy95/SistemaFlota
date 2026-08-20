namespace SistemaFlota.DTOs.Costos.OrdenCompra
{
    public class CrearOrdenCompraDto
    {
        public int ProveedorId { get; set; }

        public DateTime FechaOrden { get; set; }

        public DateTime? FechaEntrega { get; set; }

        public string FormaPago { get; set; } = string.Empty;

        public string LugarEntrega { get; set; } = string.Empty;

        public string TipoImpuesto { get; set; } = "IVA";

        public decimal PorcentajeImpuesto { get; set; } = 19;

        public string? Observaciones { get; set; }

        public List<CrearOrdenCompraDetalleDto> Detalles { get; set; } = [];
    }
}
