namespace SistemaFlota.DTOs.ComprasNoFormalizadas.OrdenesCompras
{
    public class OrdenCompraNoFormalizadaPaginadoDto
    {
        public List<OrdenCompraNoFormalizadaDto> Items { get; set; } = new();
        public int Total { get; set; }
        public int Pagina { get; set; }
        public int PageSize { get; set; }
        public int TotalPaginas => (int)Math.Ceiling((double)Total / PageSize);
    }
}
