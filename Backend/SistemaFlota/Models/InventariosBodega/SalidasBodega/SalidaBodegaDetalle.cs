using SistemaFlota.Models.InventariosBodega.EntradasBodega;
using System.ComponentModel.DataAnnotations;

namespace SistemaFlota.Models.InventariosBodega.SalidasBodega
{
        public class SalidaBodegaDetalle
        {
            public int Id { get; set; }

            [Required]
            public int SalidaBodegaId { get; set; }

            public SalidaBodega SalidaBodega { get; set; } = null!;

            [Required]
            public int InventarioBodegaItemId { get; set; }

            public InventarioBodegaItem InventarioBodegaItem { get; set; }
                = null!;

            public DateTime FechaAgregado { get; set; } = DateTime.Now;
        }
}


