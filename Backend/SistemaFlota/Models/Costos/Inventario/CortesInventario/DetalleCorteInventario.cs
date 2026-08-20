using SistemaFlota.Models.Prov_Materiales.Materiales;

namespace SistemaFlota.Models.Costos.Inventario.CortesInventario
{
    public class DetalleCorteInventario
    {
        public int Id { get; set; }
        public int CorteInventarioId { get; set; }
        public CorteInventario CorteInventario { get; set; }
        public int MaterialId { get; set; }
        public Material Material { get; set; }
        public string? Color { get; set; }
        public decimal StockSistema { get; set; }
        public decimal ConteoFisico { get; set; }
        public decimal Diferencia
        {
            get
            {
                return ConteoFisico - StockSistema;
            }
        }
    }
}
