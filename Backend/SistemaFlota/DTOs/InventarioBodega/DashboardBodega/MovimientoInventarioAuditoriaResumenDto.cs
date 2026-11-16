namespace SistemaFlota.DTOs.InventarioBodega.DashboardBodega
{
    public class MovimientoInventarioAuditoriaResumenDto
    {
        public MovimientoInventarioTotalesDto InventarioInicial { get; set; } = new();

        public MovimientoInventarioTotalesDto Entradas { get; set; } = new();

        public MovimientoInventarioTotalesDto Salidas { get; set; } = new();

        public MovimientoInventarioTotalesDto Devoluciones { get; set; } = new();

        public MovimientoInventarioTotalesDto Eliminados { get; set; } = new();

        public MovimientoInventarioTotalesDto Ajustes { get; set; } = new();

        public MovimientoInventarioTotalesDto Existencias { get; set; } = new();
    }
}
