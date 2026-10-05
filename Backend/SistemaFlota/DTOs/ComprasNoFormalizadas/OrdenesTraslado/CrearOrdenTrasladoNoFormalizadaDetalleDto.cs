namespace SistemaFlota.DTOs.ComprasNoFormalizadas.OrdenesTraslado
{
    public class CrearOrdenTrasladoNoFormalizadaDetalleDto
    {
        public int? MaterialNoFormalizadoId { get; set; }

        public string Proveedor { get; set; } = string.Empty;

        public string Tipo { get; set; } = string.Empty;

        public string Densidad { get; set; } = string.Empty;

        public string Color { get; set; } = string.Empty;

        public decimal CantidadKg { get; set; }

        public decimal Bultos { get; set; }
    }
}