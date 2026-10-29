namespace SistemaFlota.DTOs.Auditorias
{
    public class AuditoriaFiltroDto
    {
        public string? Usuario { get; set; }

        public string? Rol { get; set; }

        public string? Accion { get; set; }

        public string? Modulo { get; set; }

        public string? Resultado { get; set; }

        public DateTime? Desde { get; set; }

        public DateTime? Hasta { get; set; }
    }
}
