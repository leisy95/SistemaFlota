namespace SistemaFlota.DTOs.Flota.Inspecciones
{
    public class InspeccionDetalleResponseDto
    {
        public int Id { get; set; }
        public int? ChecklistItemId { get; set; }

        public string NombreItem { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public string? Observacion { get; set; }
        public string? FotoEvidencia { get; set; }
    }
}