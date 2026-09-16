namespace SistemaFlota.Models.Correos
{
    public class CorreoAutorizado
    {
        public int Id { get; set; }

        public int UsuarioId { get; set; }

        public string Proveedor { get; set; } = "Google";

        public string Email { get; set; } = string.Empty;

        public string RefreshToken { get; set; } = string.Empty;

        public DateTime FechaAutorizacion { get; set; }

        public bool Activo { get; set; } = true;

        public Usuario? Usuario { get; set; }
    }
}
