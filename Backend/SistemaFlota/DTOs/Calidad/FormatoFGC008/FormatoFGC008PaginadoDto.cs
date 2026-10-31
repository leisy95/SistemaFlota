namespace SistemaFlota.DTOs.Calidad.FormatoFGC008
{
    public class FormatoFGC008PaginadoDto
    {
        public List<Models.Calidad.FormatoFGC008.FormatoFGC008> Items { get; set; } = new();

        public int Total { get; set; }

        public int Pagina { get; set; }

        public int PorPagina { get; set; }

        public int TotalPaginas { get; set; }
    }
}
