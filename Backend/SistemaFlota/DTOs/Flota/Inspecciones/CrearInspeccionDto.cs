using System.ComponentModel.DataAnnotations;

namespace SistemaFlota.DTOs.Flota.Inspecciones
{
    public class CrearInspeccionDto
    {
        [Required]
        public int VehiculoId { get; set; }

        [Required]
        public int ConductorId { get; set; }

        [Range(0, int.MaxValue)]
        public int Kilometraje { get; set; }

        [Required]
        public string Checklist { get; set; } = string.Empty;

        public IFormFile? FotoOdometro { get; set; }

        public IFormFile? FirmaCondutor { get; set; }

        public List<IFormFile> Evidencias { get; set; } = new();

        public List<int> EvidenciaIndices { get; set; } = new();
    }
}
