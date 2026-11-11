namespace SistemaFlota.DTOs.InventarioBodega.Clientes
{
    public class ClienteRequestDto
    {
        public string Nombre { get; set; } = "";

        public string? Direccion { get; set; }

        public string? Telefono { get; set; }

        public string? Email { get; set; }
    }
}
