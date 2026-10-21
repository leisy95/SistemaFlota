namespace SistemaFlota.DTOs.Flota.Inspecciones
{
    public class ChecklistGuardarDto
    {
        public int? Id { get; set; }

        public string? Descripcion { get; set; }

        public string? Estado { get; set; }

        public string? Observacion { get; set; }
    }
}
