namespace SistemaFlota.DTOs.InventarioBodega.Inventario
{
    public class FiltrosInventarioDto
    {
        public List<string> Materiales { get; set; } = new();
        public List<string> Colores { get; set; } = new();
    }
}
