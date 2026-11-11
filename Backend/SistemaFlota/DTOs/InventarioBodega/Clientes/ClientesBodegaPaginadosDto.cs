namespace SistemaFlota.DTOs.InventarioBodega.Clientes
{
    public class ClientesBodegaPaginadosDto
    {
        public List<ClienteResponseDto> Items { get; set; } = new();

        public int Total { get; set; }

        public int Pagina { get; set; }

        public int PageSize { get; set; }

        public int TotalPaginas { get; set; }
    }
}