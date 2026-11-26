using System.ComponentModel.DataAnnotations;

namespace SistemaFlota.DTOs.Referencias
{
    public class InterpretarDescripcionRequestDto
    {
        [Required(ErrorMessage = "La descripción es obligatoria.")]
        [StringLength(300, ErrorMessage = "La descripción no puede superar 300 caracteres.")]
        public string Descripcion { get; set; } = string.Empty;
    }

    public class InterpretarDescripcionesLoteRequestDto
    {
        [Required]
        [MinLength(1, ErrorMessage = "Envía al menos una descripción.")]
        [MaxLength(200, ErrorMessage = "Máximo 200 descripciones por solicitud.")]
        public List<string?> Descripciones { get; set; } = new();
    }

    public class DescripcionInterpretadaDto
    {
        public string Descripcion { get; set; } = string.Empty;
        public bool Valida { get; set; }
        public List<string> Errores { get; set; } = new();
        public List<string> Advertencias { get; set; } = new();

       
        public string? Material { get; set; }
        public bool Biodegradable { get; set; }
        public bool Impreso { get; set; }

        public decimal? FuelleLateral { get; set; }
        public decimal? FuelleFondo { get; set; }
        public decimal? FuelleSuperior { get; set; }
        public decimal? Solapa { get; set; }

        
        public string? Troquel { get; set; }
        public bool CintaSeguridad { get; set; }
        public bool CintaPegaDespega { get; set; }

        public List<string> CodigosSinDefinir { get; set; } = new();
    }
}