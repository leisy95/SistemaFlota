using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaFlota.Models
{
    [Table("Cajones")]
    public class Cajon
    {
        [Key] public int Id { get; set; }
        [Required] public int Numero { get; set; }
        [MaxLength(200)] public string? Descripcion { get; set; }
        public bool Activo { get; set; } = true;
        public DateTime FechaCreacion { get; set; } = DateTime.Now;
        public ICollection<CyreleRegistro> Registros { get; set; } = new List<CyreleRegistro>();
    }

    [Table("CyreleRegistros")]
    public class CyreleRegistro
    {
        [Key] public int Id { get; set; }
        [Required] public int CajonId { get; set; }
        [Required][MaxLength(200)] public string Nombre { get; set; } = string.Empty;
        [MaxLength(255)] public string? Foto { get; set; }
        public DateTime FechaCreacion { get; set; } = DateTime.Now;
        public int CreadoPor { get; set; }
        public int? ModificadoPor { get; set; }
        public DateTime? FechaModificacion { get; set; }

        [ForeignKey("CajonId")]
        public Cajon? Cajon { get; set; }
    }
}