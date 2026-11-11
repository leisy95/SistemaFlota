namespace SistemaFlota.DTOs.InventarioBodega.EntradasBodega
{
    public class MostrarEntradaDto
    {
        public int Id { get; set; }
        public string Referencia { get; set; } = string.Empty;
        public string ReferenciaPeso { get; set; } = string.Empty;
        public DateTime FechaRegistroItem { get; set; }
        public decimal PesoActual { get; set; }
        public string Estado { get; set; } = string.Empty;
    }
}
