namespace SistemaFlota.DTOs.InventarioBodega.HistorialMovimientos
{
    public class MovimientoInventarioBodegaResumenDto
    {
        public int TotalMovimientos { get; set; }
        public int TotalCreaciones { get; set; }
        public int TotalActualizaciones { get; set; }
        public int TotalEliminaciones { get; set; }
        public int TotalConfirmaciones { get; set; }
        public int TotalCancelaciones { get; set; }
    }
}
