namespace SistemaFlota.DTOs.InventarioBodega.SalidasBodega
{
    public class SalidaBodegaActualDto
    {
        public bool Existe { get; set; }

        public string? Mensaje { get; set; }

        public int? Id { get; set; }

        public string? Estado { get; set; }

        public DateTime? FechaCreacion { get; set; }

        public List<SalidaBodegaDetalleDto> Detalles { get; set; } = new();
    }
}
