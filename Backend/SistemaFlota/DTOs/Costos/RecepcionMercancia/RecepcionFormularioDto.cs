namespace SistemaFlota.DTOs.Costos.RecepcionMercancia
{
    public class RecepcionFormularioDto
    {
        public int OrdenCompraId { get; set; }

        public string NumeroOrden { get; set; } = string.Empty;

        public string Proveedor { get; set; } = string.Empty;

        public string? Recibe { get; set; }
        public string? Cargo { get; set; }

        public DateTime FechaOrden { get; set; }

        public List<RecepcionMercanciaDetalleFormularioDto> Items { get; set; }
            = new();
    }

    public class RecepcionMercanciaDetalleFormularioDto
    {
        public int OrdenCompraDetalleId { get; set; }

        public int MaterialId { get; set; }

        public string Material { get; set; } = string.Empty;

        public decimal Cantidad { get; set; }

        public decimal Bultos { get; set; }
    }
}