namespace SistemaFlota.DTOs.Costos.DashboardEjecutivo
{
    public class FiltrosDashboardEjecutivoDto
    {
        public List<string> Estados { get; set; } = new();
        public List<string> Categorias { get; set; } = new();
        public List<string> Densidades { get; set; } = new();
        public List<string> Proveedores { get; set; } = new();
    }
}
