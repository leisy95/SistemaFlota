namespace SistemaFlota.DTOs.ComprasNoFormalizadas.OrdenesTraslado
{
    public class VerificarOrdenTrasladoNoFormalizadaDetalleDto
    {
        public int DetalleId { get; set; }
        public decimal CantidadVerificadaKg { get; set; }
        public decimal BultosVerificados { get; set; }
    }
}
