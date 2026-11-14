namespace SistemaFlota.DTOs.InventarioBodega.Inventario
{
    public class RegistrarItemResponseDto
    {
        public int IdItemGenerado { get; set; }

        public string Referencia { get; set; } = string.Empty;

        public string Message { get; set; } = string.Empty;
    }
}
