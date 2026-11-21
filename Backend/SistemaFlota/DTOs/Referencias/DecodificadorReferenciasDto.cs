using System.ComponentModel.DataAnnotations;

namespace SistemaFlota.DTOs.Referencias
{
    public class DecodificarReferenciaRequestDto
    {
        [Required(ErrorMessage = "La referencia es obligatoria.")]
        [StringLength(40, ErrorMessage = "La referencia no puede superar 40 caracteres.")]
        public string Codigo { get; set; } = string.Empty;
    }

    public class DecodificarReferenciasLoteRequestDto
    {
        [Required]
        [MinLength(1, ErrorMessage = "Envía al menos una referencia.")]
        [MaxLength(200, ErrorMessage = "Máximo 200 referencias por solicitud.")]
        public List<string?> Codigos { get; set; } = new();
    }

    public class ReferenciaDecodificadaDto
    {
        public string Codigo { get; set; } = string.Empty;
        public bool Valida { get; set; }
        public List<string> Errores { get; set; } = new();
        public List<string> Advertencias { get; set; } = new();

        public int? TipoCodigo { get; set; }
        public string? Tipo { get; set; }
        public string? Origen { get; set; }
        public string? Densidad { get; set; }

        public string? ColorCodigo { get; set; }
        public string? ColorNombre { get; set; }
        public bool Impreso { get; set; }

        public decimal? Ancho { get; set; }
        public decimal? Largo { get; set; }
        public decimal? Calibre { get; set; }

        public string? Sufijo { get; set; }
        public string? SufijoNombre { get; set; }
        public bool SufijoReconocido { get; set; }
        public bool EsPosconsumo { get; set; }
        public bool EsBiodegradable { get; set; }
        public bool EsCompostable { get; set; }
    }

    public class ItemCatalogoReferenciaDto
    {
        public string Codigo { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
    }

    public class CatalogosReferenciaDto
    {
        public List<ItemCatalogoReferenciaDto> Tipos { get; set; } = new();
        public List<ItemCatalogoReferenciaDto> Colores { get; set; } = new();
        public List<ItemCatalogoReferenciaDto> Sufijos { get; set; } = new();
    }
}