namespace SistemaFlota.DTOs.InventarioBodega.HistorialMovimientos
{
    public class MovimientoInventarioBodegaPaginadoDto
    {
        public List<MovimientoInventarioBodegaDto> Movimientos { get; set; } = new();

        public int TotalRegistros { get; set; }
        public int Pagina { get; set; }
        public int PorPagina { get; set; }
        public int TotalPaginas { get; set; }
    }
}
