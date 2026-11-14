namespace SistemaFlota.DTOs.InventarioBodega.HistorialMovimientos
{
    public class MovimientoInventarioBodegaOpcionesDto
    {
        public List<string> Modulos { get; set; } = new();
        public List<string> Usuarios { get; set; } = new();
        public List<string> Acciones { get; set; } = new();
    }
}
