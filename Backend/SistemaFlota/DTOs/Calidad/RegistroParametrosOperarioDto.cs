namespace SistemaFlota.DTOs
{
    public class GuardarParametrosOperarioDto
    {
        public string OperarioNombre { get; set; } = string.Empty;
        public string? VariablesCriticasJson { get; set; }
        public string? MotivoCambio { get; set; }
    }
}