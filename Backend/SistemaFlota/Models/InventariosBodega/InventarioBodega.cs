
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaFlota.Models.InventarioBodega
{
    public class InventarioBodega
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required, StringLength(255)]
        public string Referencia { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Descripcion { get; set; }

        [Column("Tipo de Bolsa")]
        [StringLength(255)]
        public string? TipoBolsa { get; set; }

        [Column("Tipo Material")]
        [StringLength(255)]
        public string? TipoMaterial { get; set; }

        [StringLength(100)]
        public string? Densidad { get; set; }

        [StringLength(100)]
        public string? Color { get; set; }

        [Column("Segundo color")]
        [StringLength(100)]
        public string? SegundoColor { get; set; }

        [Column("Impreso/No")]
        [StringLength(50)]
        public string? ImpresoNo { get; set; }

        [StringLength(255)]
        public string? Troquel { get; set; }

        [StringLength(500)]
        public string? ReferenciaImpresa { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal Ancho { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal Alto { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal Calibre { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal Peso { get; set; }

        public int Cantidad { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal FuelleLateral { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal FuelleFondo { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal FuelleSuperior { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal PaqueteNumero { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal PaqueteCantidad { get; set; }

        [StringLength(255)]
        public string? ReferenciaNormalizada { get; set; }

        // Relaciones
        public ICollection<InventarioBodegaItem> InventarioItems { get; set; }
            = new List<InventarioBodegaItem>();

        public ICollection<MovimientoInventarioBodega> Movimientos { get; set; }
            = new List<MovimientoInventarioBodega>();
    }
}
