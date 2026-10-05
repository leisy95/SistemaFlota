namespace SistemaFlota.DTOs.ComprasNoFormalizadas.OrdenesTraslado
{
    public class CrearOrdenTrasladoNoFormalizadaDto
    {
        public string Destino { get; set; } = string.Empty;

        public List<CrearOrdenTrasladoNoFormalizadaDetalleDto> Materiales { get; set; }
            = new();
    }
}
