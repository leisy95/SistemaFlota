namespace SistemaFlota.DTOs.Costos.Inventario
{
    public class InventarioPaginadoDto
    {
        public List<InventarioDto> Items { get; set; } = new();
        public int Total { get; set; }
        public int Pagina { get; set; }
        public int PageSize { get; set; }
    }
}
