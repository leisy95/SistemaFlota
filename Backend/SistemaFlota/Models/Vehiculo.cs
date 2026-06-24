using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaFlota
{
    public class Vehiculo
    {
        [Key]
        public int Id { get; set; }

        public string Placa { get; set; } = string.Empty;

        public string Marca { get; set; } = string.Empty;

        public string Modelo { get; set; } = string.Empty;

        public int Año { get; set; }

        public string Color { get; set; } = string.Empty;

        public string Estado { get; set; } = "Activo";

        public string? Foto { get; set; }
        public string? Tenencia { get; set; }

        // CLAVE FORÁNEA
        public int ConductorId { get; set; }

        // RELACIÓN
        [ForeignKey("ConductorId")]
        public Conductor? Conductor { get; set; }
    }
}
