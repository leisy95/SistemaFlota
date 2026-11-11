using System.ComponentModel.DataAnnotations;

namespace SistemaFlota.DTOs.InventarioBodega.SalidasBodega
{
    public class ConfirmarSalidaBodegaDto
    {
        [Range(1, int.MaxValue, ErrorMessage = "El cliente seleccionado no es válido.")]
        public int? ClienteId { get; set; }
    }
}
