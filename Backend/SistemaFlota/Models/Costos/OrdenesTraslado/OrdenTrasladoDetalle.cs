using SistemaFlota.Models.Prov_Materiales.Materiales;

namespace SistemaFlota.Models.Costos.OrdenesTraslado
{
    public class OrdenTrasladoDetalle
    {
        public int Id { get; set; }

        public int OrdenTrasladoId { get; set; }

        public int? MaterialId { get; set; }

        public string Proveedor { get; set; } = string.Empty;

        public string Tipo { get; set; } = string.Empty;

        public string Densidad { get; set; } = string.Empty;

        public string Color { get; set; } = string.Empty;

        public decimal CantidadKg { get; set; }

        public decimal Bultos { get; set; }

        public decimal? CantidadVerificadaKg { get; set; }

        public decimal? BultosVerificados { get; set; }

        public string EstadoVerificacion { get; set; } = "Pendiente";

        public virtual OrdenTraslado OrdenTraslado { get; set; } = null!;

        public virtual Material? Material { get; set; }
    }
}