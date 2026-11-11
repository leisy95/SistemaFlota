namespace SistemaFlota.DTOs.InventarioBodega.Clientes
{
    public class ClienteBodegaFiltroDto
    {
        public string? Buscar { get; set; }

        public string? Nombre { get; set; }

        public string? Telefono { get; set; }

        public string? Email { get; set; }

        public int Pagina { get; set; } = 1;

        public int PageSize { get; set; } = 10;
    }
}
