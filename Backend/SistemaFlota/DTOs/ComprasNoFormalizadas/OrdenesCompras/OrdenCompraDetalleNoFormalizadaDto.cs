namespace SistemaFlota.DTOs.ComprasNoFormalizadas.OrdenesCompras
{
    public class OrdenCompraDetalleNoFormalizadaDto
    {
        public int Id { get; set; }
        public int MaterialNoFormalizadoId { get; set; }
        public string Material { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public decimal CantidadKg { get; set; }
        public decimal KgPorBulto { get; set; }
        public decimal Bultos { get; set; }
        public decimal CostoKg { get; set; }
        public decimal Subtotal { get; set; }
    }
}
