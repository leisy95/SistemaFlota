using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaFlota.Models.Calidad.FormatoFGC008
{
    [Table("FormatoFGC008Foto")]
    public class FormatoFGC008Foto
    {
        public int Id { get; set; }

        public int FormatoFGC008Id { get; set; }

        [MaxLength(100)]
        public string Archivo { get; set; } = string.Empty;

        public DateTime FechaCreacion { get; set; } = DateTime.Now;

        [MaxLength(100)]
        public string? SubidaPor { get; set; }
    }
}