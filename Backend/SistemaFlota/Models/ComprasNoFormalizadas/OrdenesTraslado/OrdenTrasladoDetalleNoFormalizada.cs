using SistemaFlota.Models.ComprasNoFormalizadas.Materiales;

namespace SistemaFlota.Models.ComprasNoFormalizadas.OrdenesTraslado
{
    public class OrdenTrasladoDetalleNoFormalizada
    {
        public int Id { get; set; }

        public int OrdenTrasladoNoFormalizadaId { get; set; }

        public int? MaterialNoFormalizadoId { get; set; }

        public string Proveedor { get; set; } = string.Empty;

        public string Tipo { get; set; } = string.Empty;

        public string Densidad { get; set; } = string.Empty;

        public string Color { get; set; } = string.Empty;

        public decimal CantidadKg { get; set; }

        public decimal Bultos { get; set; }

        public decimal? CantidadVerificadaKg { get; set; }

        public decimal? BultosVerificados { get; set; }

        public string EstadoVerificacion { get; set; } = "Pendiente";

        public virtual OrdenTrasladoNoFormalizada OrdenTrasladoNoFormalizada { get; set; } = null!;

        public virtual MaterialNoFormalizado? MaterialNoFormalizado { get; set; }
    }
}
