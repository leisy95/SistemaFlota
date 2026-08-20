namespace SistemaFlota.DTOs.Prov_Materiales.Proveedores
{
    public class ProveedorPaginadoDto
    {
        public int TotalRegistros { get; set; }
        public int Pagina { get; set; }
        public int TamanoPagina { get; set; }
        public int TotalPaginas { get; set; }
        public List<ProveedorDto> Datos { get; set; } = [];
    }
}
