using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaFlota
{
    [Table("CostosFletes")]
    public class CosteFlete
    {
        public int Id { get; set; }
        public int AutorizacionId { get; set; }
        public DateTime FechaRegistro { get; set; } = DateTime.Now;
        public decimal Peajes { get; set; } = 0;
        public decimal Combustible { get; set; } = 0;
        public decimal Parqueos { get; set; } = 0;
        public decimal DescarguesMcia { get; set; } = 0;
        public decimal CargueMateriales { get; set; } = 0;
        public decimal Alimentacion { get; set; } = 0;
        public decimal Hospedaje { get; set; } = 0;
        public decimal Varios { get; set; } = 0;
        public decimal Total { get; set; } = 0;
        public string? Observaciones { get; set; }
        public string Estado { get; set; } = "Pendiente";
        public string? VerificadoPor { get; set; }
        public string? FirmaVerificacion { get; set; }
        public DateTime? FechaVerificacion { get; set; }

        [ForeignKey("AutorizacionId")]
        public Autorizacion? Autorizacion { get; set; }
    }
}