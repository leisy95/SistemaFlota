using System.ComponentModel.DataAnnotations;

namespace SistemaFlota
{
    public class TipoVehiculo
    {
        [Key]
        public int Id { get; set; }

        public string Nombre { get; set; }
            = string.Empty;
    }
}