namespace SistemaFlota.DTOs.Auditorias
{
    public class AuditoriaPaginadaDto
    {
        public List<AuditoriaDto> Registros { get; set; } = new();

        public int TotalRegistros { get; set; }

        public int Pagina { get; set; }

        public int PorPagina { get; set; }

        public int TotalPaginas { get; set; }
    }
}
