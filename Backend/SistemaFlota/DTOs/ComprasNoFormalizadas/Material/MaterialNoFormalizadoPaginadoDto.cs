namespace SistemaFlota.DTOs.ComprasNoFormalizadas.Material
{
    public class MaterialNoFormalizadoPaginadoDto
    {
        public int TotalRegistros { get; set; }

        public int Pagina { get; set; }

        public int TamanoPagina { get; set; }

        public int TotalPaginas { get; set; }

        public List<MaterialNoFormalizadoDto> Datos { get; set; }
            = new();
    }
}
