using System.ComponentModel.DataAnnotations;

namespace SistemaFlota.DTOs.InventarioBodega.Devoluciones
{
    public class DevolucionEntradaDto
    {
        [Required]
        public int InventarioBodegaItemId { get; set; }

        [StringLength(500)]
        public string? Observacion { get; set; }
    }
}
