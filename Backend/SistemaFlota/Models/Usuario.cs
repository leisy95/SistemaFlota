using System.ComponentModel.DataAnnotations;
namespace SistemaFlota
{
    public class Usuario
    {
        [Key]
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty; // ← se mantiene por compatibilidad
        public string? PasswordHash { get; set; }            // ← nuevo campo BCrypt
        public string Rol { get; set; } = "Admin";
        public bool Activo { get; set; } = true;
        public string? Email { get; set; }
        public string? TokenRecuperacion { get; set; }
        public DateTime? TokenExpiracion { get; set; }
        public List<UsuarioPermiso> Permisos { get; set; } = new();
        public string? PasswordConductores { get; set; }
    }
}