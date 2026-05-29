using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace SistemaFlota
{
    public class InfraccionConductor
    {
        [Key]
        public int Id { get; set; }

        public int ConductorId { get; set; }
        [ForeignKey("ConductorId")]
        public Conductor? Conductor { get; set; }

        public DateTime FechaInfraccion { get; set; }
        // Velocidad | Semáforo | Documentos | Alcoholemia | Estacionamiento | Carga | Otro
        public string TipoInfraccion { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public string? Lugar { get; set; }
        public decimal? Valor { get; set; }
        // Pendiente | Pagada | Apelada | Condonada
        public string Estado { get; set; } = "Pendiente";
        public string? NumeroComparendo { get; set; }
        public string? Observaciones { get; set; }
        public string? Documento { get; set; }

        public DateTime FechaRegistro { get; set; } = DateTime.Now;
    }
}