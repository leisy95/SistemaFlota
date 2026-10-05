namespace SistemaFlota.DTOs.ComprasNoFormalizadas.RecepcionMercancias
{
    public class FiltrosRecepcionMercanciaNoFormalizadaDto
    {
        public List<FiltroProveedorNoFormalizadoDto> Proveedores { get; set; }
            = new();
    }

    public class FiltroProveedorNoFormalizadoDto
    {
        public int Id { get; set; }

        public string Nombre { get; set; } = string.Empty;
    }
}
