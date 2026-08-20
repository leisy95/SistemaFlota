namespace SistemaFlota.DTOs.Costos.OrdenesTraslado
{
    public class CrearOrdenTrasladoDto
    {
        public string Destino { get; set; } = string.Empty;

        public List<CrearOrdenTrasladoDetalleDto> Materiales { get; set; }
            = new();
    }
}
