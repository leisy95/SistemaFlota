namespace SistemaFlota.DTOs.Costos.HistorialMovimientos
{
    public class MovimientoHistorialPaginadoDto
    {
        public List<MovimientoHistorialDto> Movimientos { get; set; } = new();
        public int TotalRegistros { get; set; }
        public int Pagina { get; set; }
        public int PorPagina { get; set; }
        public int TotalPaginas { get; set; }
    }
}
