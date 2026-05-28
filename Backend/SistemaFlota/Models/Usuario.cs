using System.ComponentModel.DataAnnotations;

namespace SistemaFlota
{
    public class Usuario
    {
        [Key]
        public int Id { get; set; }

        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Rol { get; set; } = "Admin";
        public bool Activo { get; set; } = true;

        // NUEVO
        public string? Email { get; set; }
        public string? TokenRecuperacion { get; set; }
        public DateTime? TokenExpiracion { get; set; }

        // PERMISOS
        public List<UsuarioPermiso> Permisos { get; set; } = new();
    }
}