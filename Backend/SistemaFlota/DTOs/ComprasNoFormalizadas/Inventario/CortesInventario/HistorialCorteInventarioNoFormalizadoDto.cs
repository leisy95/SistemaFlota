namespace SistemaFlota.DTOs.ComprasNoFormalizadas.Inventario.CortesInventario
{
    public class HistorialCorteInventarioNoFormalizadoDto
    {
        public int Id { get; set; }

        public DateTime Fecha { get; set; }

        public string Estado { get; set; } = string.Empty;

        public string Usuario { get; set; } = string.Empty;

        public int CantidadDetalles { get; set; }
    }
}