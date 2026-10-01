namespace SistemaFlota.DTOs.ComprasNoFormalizadas.Inventario.CortesInventario
{
    public class HistorialCorteDetalleNoFormalizadoDto
    {
        public int Id { get; set; }

        public DateTime Fecha { get; set; }

        public string Estado { get; set; } = string.Empty;

        public string Usuario { get; set; } = string.Empty;

        public List<DetalleHistorialCorteNoFormalizadoDto> Detalles { get; set; }
            = new();
    }
}