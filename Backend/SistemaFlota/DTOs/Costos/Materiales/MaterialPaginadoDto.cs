using SistemaFlota.DTOs.Prov_Materiales.Proveedores;

namespace SistemaFlota.DTOs.Costos.Materiales
{
    public class MaterialPaginadoDto
    {
        public int TotalRegistros { get; set; }
        public int Pagina { get; set; }
        public int TamanoPagina { get; set; }
        public int TotalPaginas { get; set; }
        public List<MaterialDto> Datos { get; set; } = [];
    }
}
