namespace SistemaFlota.DTOs.HistorialVersionOdo
{
    public class PaginacionDto<T>
    {
        public List<T> Datos { get; set; } = new();

        public int Pagina { get; set; }

        public int PorPagina { get; set; }

        public int TotalRegistros { get; set; }

        public int TotalPaginas { get; set; }
    }
}
