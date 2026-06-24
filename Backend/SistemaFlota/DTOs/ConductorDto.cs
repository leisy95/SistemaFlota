namespace SistemaFlota.DTOs
{
    public class ConductorDto
    {
        public int Id { get; set; }

        public string Nombre { get; set; } = string.Empty;

        public string Licencia { get; set; } = string.Empty;

        public string Telefono { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        // 🔥 IMPORTANTE
        public string? Foto { get; set; }
        public string? ClasificacionVehiculo { get; set; }
        public string? TipoVehiculo { get; set; }
    }
}