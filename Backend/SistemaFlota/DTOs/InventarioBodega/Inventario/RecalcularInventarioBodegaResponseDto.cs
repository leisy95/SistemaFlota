namespace SistemaFlota.DTOs.InventarioBodega.Inventario
{
    public class RecalcularInventarioBodegaResponseDto
    {
        public string Message { get; set; } = string.Empty;

        public int TotalInventarios { get; set; }

        public int InventariosActualizados { get; set; }
    }
}
