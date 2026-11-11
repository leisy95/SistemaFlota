using System.ComponentModel.DataAnnotations;

namespace SistemaFlota.DTOs.InventarioBodega.Devoluciones
{
    public class AjusteGeneralEntradaDto
    {
        [Required]
        [MinLength(1)]
        public List<int> ItemIds { get; set; } = new();

        [Required]
        [StringLength(500)]
        public string Observacion { get; set; } = string.Empty;
    }
}
