namespace SistemaFlota.DTOs.Costos.OrdenesTraslado
{
    public class VerificarOrdenTrasladoDto
    {
        public int OrdenTrasladoId { get; set; }

        public string? Observaciones { get; set; }

        public List<VerificarOrdenTrasladoDetalleDto> Materiales { get; set; } = new();
    }
}
