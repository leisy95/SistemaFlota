namespace SistemaFlota.DTOs.InventarioBodega.Clientes
{
    public class EditarClienteRequestDto
    {
        public int Id { get; set; }

        public string Nombre { get; set; } = "";

        public string? Direccion { get; set; }

        public string? Telefono { get; set; }

        public string? Email { get; set; }
    }
}
