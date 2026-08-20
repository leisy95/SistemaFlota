namespace SistemaFlota.DTOs.Costos.RecepcionMercancia
{
    public class RecepcionMercanciaPaginadoDto
    {
        public List<RecepcionMercanciaDto> Data { get; set; }
            = new();

        public int TotalRegistros { get; set; }

        public int PaginaActual { get; set; }

        public int TotalPaginas { get; set; }
    }
}