namespace SistemaFlota.DTOs.ComprasNoFormalizadas.Proveedores
{
    public class ProveedorNoFormalizadoPaginadoDto
    {
        public int TotalRegistros { get; set; }

        public int Pagina { get; set; }

        public int TamanoPagina { get; set; }

        public int TotalPaginas { get; set; }

        public List<ProveedorNoFormalizadoDto> Datos { get; set; }
            = new List<ProveedorNoFormalizadoDto>();
    }
}
