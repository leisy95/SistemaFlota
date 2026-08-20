namespace SistemaFlota.DTOs.Costos.Inventario
{
    public class InventarioAjusteDto
    {
        public int InventarioId { get; set; }

        public string Material { get; set; } = string.Empty;

        public string Color { get; set; } = string.Empty;

        public decimal StockActual { get; set; }

        public decimal CostoPromedio { get; set; }
    }
}
