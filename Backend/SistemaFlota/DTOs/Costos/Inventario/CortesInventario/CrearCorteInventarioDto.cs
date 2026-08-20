namespace SistemaFlota.DTOs.Costos.Inventario.CortesInventario
{
    public class CrearCorteInventarioDto
    {
        public List<DetalleCorteInventarioDto> Detalles { get; set; } = new();
    }


    public class DetalleCorteInventarioDto
    {
        public int MaterialId { get; set; }

        public string? Color { get; set; }

        public decimal Conteo { get; set; }
    }
}