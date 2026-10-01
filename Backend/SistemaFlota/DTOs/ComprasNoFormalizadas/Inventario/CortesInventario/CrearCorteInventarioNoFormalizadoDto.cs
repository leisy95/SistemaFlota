namespace SistemaFlota.DTOs.ComprasNoFormalizadas.Inventario.CortesInventario
{
    public class CrearCorteInventarioNoFormalizadoDto
    {
        public List<DetalleCorteInventarioNoFormalizadoDto> Detalles { get; set; }
            = new();
    }

    public class DetalleCorteInventarioNoFormalizadoDto
    {
        public int MaterialId { get; set; }

        public string? Color { get; set; }

        public decimal Conteo { get; set; }
    }
}