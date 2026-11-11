namespace SistemaFlota.DTOs.InventarioBodega.SalidasBodega
{
    public class SalidaBodegaDetalleDto
    {
        public int IdInventarioBodegaItem { get; set; }

        public string ReferenciaPeso { get; set; } = string.Empty;

        public decimal PesoActual { get; set; }
    }
}
