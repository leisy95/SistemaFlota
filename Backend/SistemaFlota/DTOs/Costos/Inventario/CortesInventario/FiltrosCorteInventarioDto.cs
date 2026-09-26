namespace SistemaFlota.DTOs.Costos.Inventario.CortesInventario
{
    public class FiltrosCorteInventarioDto
    {
        public List<string> Proveedores { get; set; } = new();
        public List<MaterialFiltroCorteDto> Materiales { get; set; } = new();
    }

    public class MaterialFiltroCorteDto
    {
        public int MaterialId { get; set; }
        public string Material { get; set; } = string.Empty;
        public string Proveedor { get; set; } = string.Empty;
    }
}
