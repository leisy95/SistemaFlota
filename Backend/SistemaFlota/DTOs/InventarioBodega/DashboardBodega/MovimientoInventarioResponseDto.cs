namespace SistemaFlota.DTOs.InventarioBodega.DashboardBodega
{
    public class MovimientoInventarioResponseDto
    {
        public int Id { get; set; }
        public string Referencia { get; set; } = string.Empty;
        public string ReferenciaPeso { get; set; } = string.Empty;
        public decimal Peso { get; set; }
        public DateTime Fecha { get; set; }
        public string Tipo { get; set; } = string.Empty;
        public string Usuario { get; set; } = string.Empty;
        public string? ReferenciaImpresa { get; set; }
        public string? InventarioNombre { get; set; }
        public string? InventarioItemCodigo { get; set; }
    }
}
