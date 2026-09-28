namespace SistemaFlota.DTOs.ComprasNoFormalizadas.OrdenesCompras
{
    public class CrearOrdenCompraDetalleNoFormalizadaDto
    {
        public int MaterialNoFormalizadoId { get; set; }
        public string Color { get; set; } = string.Empty;
        public decimal CantidadKg { get; set; }
        public decimal KgPorBulto { get; set; }
        public decimal CostoKg { get; set; }
    }
}
