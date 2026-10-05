namespace SistemaFlota.DTOs.ComprasNoFormalizadas.OrdenesTraslado
{
    public class OrdenTrasladoNoFormalizadaPaginadoDto
    {
        public List<OrdenTrasladoNoFormalizadaDto> Datos { get; set; } = new();
        public int TotalRegistros { get; set; }
        public int Pagina { get; set; }
        public int TamanoPagina { get; set; }
        public int TotalPaginas { get; set; }
    }
}
