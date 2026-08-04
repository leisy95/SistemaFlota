using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace SistemaFlota.Models
{
    public class OpcionFormulario
    {
        [Key] public int Id { get; set; }
        [Required][MaxLength(50)] public string Categoria { get; set; } = string.Empty; // "Maquina", "Corona", "Molde", "Operario"
        public int? TipoFormatoId { get; set; } // null = aplica a todos los formatos
        [Required][MaxLength(200)] public string Valor { get; set; } = string.Empty;
        public int Orden { get; set; } = 0;
        public bool Activo { get; set; } = true;

        [ForeignKey("TipoFormatoId")]
        public TipoFormatoCalidad? TipoFormato { get; set; }
    }
}