using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
namespace SistemaFlota
{
    public class PedidoReferencia
    {
        [Key]
        public int Id { get; set; }

        public int PedidoId { get; set; }

        [JsonIgnore]
        public Pedido? Pedido { get; set; }

        public string Referencia { get; set; } = string.Empty;
        public decimal? CantidadKg { get; set; }
        public decimal? CantidadUnidades { get; set; }
    }
}