using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaFlota
{
    public class UsuarioPermiso
    {
        [Key]
        public int Id { get; set; }

        public int UsuarioId { get; set; }
        [ForeignKey("UsuarioId")]
        public Usuario? Usuario { get; set; }

        public string Modulo { get; set; } = string.Empty;
        public bool PuedeVer { get; set; } = true;
        public bool PuedeCrear { get; set; } = false;
        public bool PuedeEditar { get; set; } = false;
        public bool PuedeEliminar { get; set; } = false;
    }
}