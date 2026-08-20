namespace SistemaFlota.DTOs.Costos.OrdenesTraslado
{
    public class OrdenTrasladoPaginadoDto
    {
        public List<OrdenTrasladoDto> Datos { get; set; } = new();
        public int TotalRegistros { get; set; }
        public int Pagina { get; set; }
        public int TamanoPagina { get; set; }
        public int TotalPaginas { get; set; }
    }
}
