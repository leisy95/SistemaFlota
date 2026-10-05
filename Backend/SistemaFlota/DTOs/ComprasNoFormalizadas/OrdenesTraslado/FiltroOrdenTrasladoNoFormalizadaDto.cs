namespace SistemaFlota.DTOs.ComprasNoFormalizadas.OrdenesTraslado
{
    public class FiltroOrdenTrasladoNoFormalizadaDto
    {
        public string? Search { get; set; }
        public string? Estado { get; set; }
        public string? Destino { get; set; }
        public int? UsuarioId { get; set; }
        public DateTime? FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }
        public int Pagina { get; set; } = 1;
        public int TamanoPagina { get; set; } = 10;
    }
}
