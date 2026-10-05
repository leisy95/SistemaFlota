namespace SistemaFlota.DTOs.ComprasNoFormalizadas.OrdenesTraslado
{
    public class VerificarOrdenTrasladoNoFormalizadaDto
    {
        public int OrdenTrasladoNoFormalizadaId { get; set; }
        public string? Observaciones { get; set; }
        public List<VerificarOrdenTrasladoNoFormalizadaDetalleDto> Materiales { get; set; } = new();
    }
}
