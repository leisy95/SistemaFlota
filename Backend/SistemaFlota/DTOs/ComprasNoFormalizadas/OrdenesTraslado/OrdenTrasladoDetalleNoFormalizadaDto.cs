namespace SistemaFlota.DTOs.ComprasNoFormalizadas.OrdenesTraslado
{
    public class OrdenTrasladoDetalleNoFormalizadaDto
    {
        public int Id { get; set; }
        public int? MaterialNoFormalizadoId { get; set; }
        public string Material { get; set; } = string.Empty;
        public string Proveedor { get; set; } = string.Empty;
        public string Tipo { get; set; } = string.Empty;
        public string Densidad { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public decimal CantidadKg { get; set; }
        public decimal Bultos { get; set; }
        public decimal? CantidadVerificadaKg { get; set; }
        public decimal? BultosVerificados { get; set; }
        public string EstadoVerificacion { get; set; } = "Pendiente";
    }
}
