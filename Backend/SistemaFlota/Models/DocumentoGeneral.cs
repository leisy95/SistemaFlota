using System.ComponentModel.DataAnnotations;

namespace SistemaFlota
{
    public class DocumentoGeneral
    {
        [Key]
        public int Id { get; set; }

        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public string Categoria { get; set; } = "General";
        public string ArchivoUrl { get; set; } = string.Empty;
        public string? Extension { get; set; }
        public long TamanoBytes { get; set; }
        public DateTime FechaSubida { get; set; } = DateTime.Now;
        public DateTime? FechaVencimiento { get; set; }
        public bool Activo { get; set; } = true;
    }
}