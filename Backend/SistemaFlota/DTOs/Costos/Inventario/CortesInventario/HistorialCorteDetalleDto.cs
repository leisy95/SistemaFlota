namespace SistemaFlota.DTOs.Costos.Inventario.CortesInventario
{
    public class HistorialCorteDetalleDto
    {
        public int Id { get; set; }
        public DateTime Fecha { get; set; }
        public string Estado { get; set; } = string.Empty;
        public string Usuario { get; set; } = string.Empty;
        public List<DetalleHistorialCorteDto> Detalles { get; set; } = new();
    }
}
