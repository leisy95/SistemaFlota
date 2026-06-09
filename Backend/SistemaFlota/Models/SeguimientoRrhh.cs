using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaFlota.Models
{
    [Table("SeguimientosRrhh")]
    public class SeguimientoRrhh
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Area { get; set; } = string.Empty;

        [Required]
        [Range(1, 12)]
        public byte Mes { get; set; }

        [Required]
        public short Anio { get; set; }

        [Required]
        [MaxLength(150)]
        public string Fuente { get; set; } = string.Empty;

        [MaxLength(150)]
        public string? Areas { get; set; }

        [Required]
        public string Descripcion { get; set; } = string.Empty;

        public string? PlanAccionSugerido { get; set; }

        [MaxLength(150)]
        public string? FactorRiesgo { get; set; }

        [Required]
        [MaxLength(10)]
        public string Prioridad { get; set; } = "Media";

        [MaxLength(150)]
        public string? Responsable { get; set; }

        public DateTime? FechaEjecucion { get; set; }
        public DateTime? FechaSeguimiento { get; set; }

        [Required]
        [MaxLength(20)]
        public string Estado { get; set; } = "Pendiente";

        public string? Observaciones { get; set; }

        [Required]
        public int CreadoPor { get; set; }
        public DateTime FechaCreacion { get; set; } = DateTime.Now;
        public int? ModificadoPor { get; set; }
        public DateTime? FechaModificacion { get; set; }

        // Navegación
        public ICollection<SeguimientoRrhhFoto> Fotos { get; set; } = new List<SeguimientoRrhhFoto>();
    }

    [Table("SeguimientosRrhhFotos")]
    public class SeguimientoRrhhFoto
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int SeguimientoId { get; set; }

        [Required]
        [MaxLength(255)]
        public string NombreArchivo { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        public string TipoFoto { get; set; } = "evidencia"; // evidencia | seguimiento

        public DateTime FechaSubida { get; set; } = DateTime.Now;

        // Navegación
        [ForeignKey("SeguimientoId")]
        public SeguimientoRrhh? Seguimiento { get; set; }
    }
}