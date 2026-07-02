namespace SistemaFlota
{
    public class NumeroEmergencia
    {
        public int Id { get; set; }
        public string Ciudad { get; set; } = string.Empty;
        public string Tipo { get; set; } = string.Empty;
        public string Numero { get; set; } = string.Empty;
        public string? Observaciones { get; set; }
        public bool Activo { get; set; } = true;
    }
}
