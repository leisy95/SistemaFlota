namespace SistemaFlota.DTOs.ComprasNoFormalizadas.Inventario
{
    public class CrearAjusteInventarioNoFormalizadoDto
    {
        public int InventarioId { get; set; }

        public string Tipo { get; set; } = string.Empty;

        public decimal Cantidad { get; set; }

        public string Motivo { get; set; } = string.Empty;

        public string? Observaciones { get; set; }
    }
}