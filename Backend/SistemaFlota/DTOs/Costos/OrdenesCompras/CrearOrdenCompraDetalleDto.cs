namespace SistemaFlota.DTOs.Costos.OrdenCompra
{
    public class CrearOrdenCompraDetalleDto
    {
        public int MaterialId { get; set; }

        public string Color { get; set; } = string.Empty;

        public decimal CantidadKg { get; set; }

        public decimal KgPorBulto { get; set; }

        public decimal CostoKg { get; set; }
    }
}
