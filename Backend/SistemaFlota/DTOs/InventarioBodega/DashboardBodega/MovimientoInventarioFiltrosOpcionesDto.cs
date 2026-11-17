namespace SistemaFlota.DTOs.InventarioBodega.DashboardBodega
{
    public class MovimientoInventarioFiltrosOpcionesDto
    {
        public List<string> Tipos { get; set; } = new();
        public List<string> Usuarios { get; set; } = new();
        public List<string> Clientes { get; set; } = new();
    }
}
