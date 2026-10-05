namespace SistemaFlota.DTOs.ComprasNoFormalizadas.RecepcionMercancias
{
    public class RecepcionMercanciaNoFormalizadaPaginadoDto
    {
        public List<RecepcionMercanciaNoFormalizadaDto> Data { get; set; }
            = new();

        public int TotalRegistros { get; set; }

        public int PaginaActual { get; set; }

        public int TotalPaginas { get; set; }
    }
}
