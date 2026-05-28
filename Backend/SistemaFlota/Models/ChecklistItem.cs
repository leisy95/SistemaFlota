using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaFlota
{
    public class ChecklistItem
    {
        [Key]
        public int Id { get; set; }

        // DESCRIPCIÓN DEL CHECK
        public string Descripcion { get; set; }
            = string.Empty;

        // ACTIVO O INACTIVO
        public bool Activo { get; set; }
            = true;

        // RELACIÓN TIPO VEHÍCULO
        public int TipoVehiculoId { get; set; }

        [ForeignKey("TipoVehiculoId")]
        public TipoVehiculo? TipoVehiculo
        { get; set; }
    }
}