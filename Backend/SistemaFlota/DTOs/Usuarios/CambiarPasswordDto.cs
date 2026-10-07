namespace SistemaFlota.DTOs.Usuarios
{
    public class CambiarPasswordDto
    {
        public string Email { get; set; } = string.Empty;

        public string Token { get; set; } = string.Empty;

        public string NuevaPassword { get; set; } = string.Empty;
    }
}
