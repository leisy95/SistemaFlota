using SistemaFlota.Models.ComprasNoFormalizadas.Materiales;

namespace SistemaFlota.Models.ComprasNoFormalizadas.Inventario.CortesInventario
{
    public class DetalleCorteInventarioNoFormalizado
    {
        public int Id { get; set; }

        public int CorteInventarioId { get; set; }

        public CorteInventarioNoFormalizado CorteInventario { get; set; } = null!;

        public int MaterialId { get; set; }

        public MaterialNoFormalizado Material { get; set; } = null!;

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
