namespace SistemaFlota.DTOs.Calidad.Formatos
{
    public class TipoFormatoCalidadDto
    {
        public int Id { get; set; }

        public string Codigo { get; set; } = string.Empty;

        public string Nombre { get; set; } = string.Empty;

        public bool TieneVariablesCriticas { get; set; }
    }
}