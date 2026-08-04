using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace SistemaFlota.Models
{
    public class CaracteristicaFormato
    {
        [Key] public int Id { get; set; }
        [Required] public int TipoFormatoId { get; set; }
        public int Orden { get; set; }
        [Required][MaxLength(200)] public string Descripcion { get; set; } = string.Empty;

        [ForeignKey("TipoFormatoId")]
        public TipoFormatoCalidad? TipoFormato { get; set; }
    }
}