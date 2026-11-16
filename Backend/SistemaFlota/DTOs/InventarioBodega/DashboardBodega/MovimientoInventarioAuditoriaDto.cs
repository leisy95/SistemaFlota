namespace SistemaFlota.DTOs.InventarioBodega.DashboardBodega
{
    public class MovimientoInventarioAuditoriaDto
    {
        public string Referencia { get; set; } = string.Empty;

        public string ReferenciaPeso { get; set; } = string.Empty;

        public string Tipo { get; set; } = string.Empty;

        public decimal Peso { get; set; }

        public DateTime Fecha { get; set; }

        public string Usuario { get; set; } = string.Empty;

        public string? Cliente { get; set; }

        public string? ReferenciaImpresa { get; set; }

        public string? Observacion { get; set; }
    }
}
