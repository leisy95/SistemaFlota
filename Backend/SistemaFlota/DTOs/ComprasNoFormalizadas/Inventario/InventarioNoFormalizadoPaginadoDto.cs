namespace SistemaFlota.DTOs.ComprasNoFormalizadas.Inventario
{
    public class InventarioNoFormalizadoPaginadoDto
    {
        public List<InventarioNoFormalizadoDto> Items { get; set; } = new();

        public int Total { get; set; }

        public int Pagina { get; set; }

        public int PageSize { get; set; }

        // Datos numéricos protegidos por permiso
        public decimal? TotalKg { get; set; }

        public decimal? TotalValorInventario { get; set; }
    }
}