namespace SistemaFlota.DTOs.Costos.Inventario.CortesInventario
{
    public class DetalleHistorialCorteDto
    {
        public int MaterialId { get; set; }
        public string Material { get; set; } = string.Empty;
        public string Proveedor { get; set; } = string.Empty;
        public string? Color { get; set; }
        public decimal StockSistema { get; set; }
        public decimal ConteoFisico { get; set; }
        public decimal Diferencia { get; set; }
    }
}
