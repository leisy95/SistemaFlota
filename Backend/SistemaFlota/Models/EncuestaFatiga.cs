using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaFlota
{
    public class EncuestaFatiga
    {
        [Key]
        public int Id { get; set; }

        public DateTime Fecha { get; set; } = DateTime.Now;

        // CONDUCTOR
        public int ConductorId { get; set; }
        [ForeignKey("ConductorId")]
        public Conductor? Conductor { get; set; }

        // VEHÍCULO
        public int VehiculoId { get; set; }
        [ForeignKey("VehiculoId")]
        public Vehiculo? Vehiculo { get; set; }

        // PREGUNTAS — true = Sí, false = No
        // 1. ¿Durmió menos de 7 horas?
        public bool DurmioMenos7Horas { get; set; }

        // 2. ¿Se siente cansado o con sueño?
        public bool SienteCansancio { get; set; }

        // 3. ¿Se despertó varias veces durante la noche?
        public bool DespertoVariasVeces { get; set; }

        // 4. ¿Está tomando medicamento que produzca sueño?
        public bool MedicamentoSueno { get; set; }

        // 5. ¿Siente dificultad para concentrarse?
        public bool DificultadConcentracion { get; set; }

        // CAMPO ADICIONAL LIBRE
        public string? OtraObservacion { get; set; }

        // RESULTADO
        // Apto | No Apto
        public string Resultado { get; set; } = "Apto";

        // QUIEN REGISTRÓ
        public string? RegistradoPor { get; set; }

        // OBSERVACIONES GENERALES
        public string? Observaciones { get; set; }
    }
}
