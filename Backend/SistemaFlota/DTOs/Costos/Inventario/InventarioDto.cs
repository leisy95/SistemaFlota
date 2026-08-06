namespace SistemaFlota.DTOs.Costos.Inventario
{
    public class InventarioDto
    {
        public int Id { get; set; }

        public int MaterialId { get; set; }

        public string Material { get; set; } = string.Empty;

        public string Proveedor { get; set; } = string.Empty;

        public string Tipo { get; set; } = string.Empty;

        public string Color { get; set; } = string.Empty;

        public string Densidad { get; set; } = string.Empty;

        public decimal StockActual { get; set; }

        public decimal CostoPromedio { get; set; }

        public decimal ValorInventario { get; set; }
    }
}
