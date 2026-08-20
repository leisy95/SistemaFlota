namespace SistemaFlota.DTOs.Costos.OrdenCompra
{
    public class OrdenCompraPaginadoDto
    {
        public List<OrdenCompraDto> Items { get; set; } = [];

        public int Total { get; set; }

        public int Pagina { get; set; }

        public int Paginas { get; set; }

        public int PageSize { get; set; }
    }
}
