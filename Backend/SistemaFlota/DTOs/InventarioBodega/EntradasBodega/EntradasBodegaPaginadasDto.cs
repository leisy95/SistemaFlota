namespace SistemaFlota.DTOs.InventarioBodega.EntradasBodega
{
    public class EntradasBodegaPaginadasDto
    {
        public List<MostrarEntradaDto> Items { get; set; } = new();

        public int Total { get; set; }
        public int Pagina { get; set; }
        public int PageSize { get; set; }
        public int TotalPaginas { get; set; }
    }
}
