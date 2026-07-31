namespace SistemaFlota.DTOs.Costos.RecepcionMercancia
{
    public class FiltrosRecepcionMercanciaDto
    {
        public List<FiltroProveedorDto> Proveedores { get; set; }
            = new();
    }

    public class FiltroProveedorDto
    {
        public int Id { get; set; }

        public string Nombre { get; set; } = string.Empty;
    }
}