namespace SistemaFlota.DTOs.InventarioBodega.DashboardBodega
{
    public class MovimientoInventarioRequestDto
    {
        public string Referencia { get; set; } = string.Empty;

        public string ReferenciaPeso { get; set; } = string.Empty;

        public decimal Peso { get; set; }

        public DateTime Fecha { get; set; }

        public string Tipo { get; set; } = string.Empty;

        public string Usuario { get; set; } = string.Empty;

        public int IdInventarioBodega { get; set; }

        public int IdInventarioBodegaItem { get; set; }

        public int? ClienteId { get; set; }

        public string? Observacion { get; set; }
    }
}