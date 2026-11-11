namespace SistemaFlota.DTOs.InventarioBodega.Inventario
{
    public class ResumenInventarioDto
    {
        public int TotalProductos { get; set; }
        public decimal PesoTotal { get; set; }
        public int CantidadTotal { get; set; }
        public int TotalColores { get; set; }
        public int TotalMateriales { get; set; }
        public int TotalReferencias { get; set; }
    }
}
