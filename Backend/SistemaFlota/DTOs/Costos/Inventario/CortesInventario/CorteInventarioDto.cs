namespace SistemaFlota.DTOs.Costos.Inventario.CortesInventario
{
    public class CorteInventarioDto
    {
        public int InventarioId { get; set; }

        public int MaterialId { get; set; }

        public string Material { get; set; } = string.Empty;

        public string Proveedor { get; set; } = string.Empty;

        public string Color { get; set; } = string.Empty;

        public decimal Sistema { get; set; }

        public decimal Conteo { get; set; }
    }
}
