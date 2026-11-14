namespace SistemaFlota.DTOs.InventarioBodega.EntradasBodega
{
    public class FiltroEntradaBodegaDto
    {
        public string? Estado { get; set; }
        public string? Buscar { get; set; }

        public DateTime? FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }

        public int Pagina { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
