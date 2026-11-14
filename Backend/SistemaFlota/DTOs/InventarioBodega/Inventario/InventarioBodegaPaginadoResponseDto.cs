using SistemaFlota.DTOs.InventarioBodega.Inventario;
using InventarioBodegaModel =
    SistemaFlota.Models.InventariosBodega.Inventario.InventarioBodega;

namespace SistemaFlota.DTOs.InventarioBodega
{
    public class InventarioBodegaPaginadoResponseDto
    {
        public ResumenInventarioDto Resumen { get; set; } = new();

        public FiltrosInventarioDto Filtros { get; set; } = new();

        public List<InventarioBodegaModel> Items { get; set; } = new();

        public int Total { get; set; }
        public int Pagina { get; set; }
        public int PageSize { get; set; }
        public int TotalPaginas { get; set; }
    }
}