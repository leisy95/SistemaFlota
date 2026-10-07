namespace SistemaFlota.DTOs.Usuarios
{
    public class CrearUsuarioDto
    {
        public string Username { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;

        public string Rol { get; set; } = string.Empty;

        public string? Nombres { get; set; }

        public string? Apellidos { get; set; }

        public string? Telefono { get; set; }

        public string? Email { get; set; }

        public bool Activo { get; set; } = true;

        public List<PermisoDto>? Permisos { get; set; }
    }
}
