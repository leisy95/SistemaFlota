namespace SistemaFlota.DTOs.InventarioBodega.DashboardBodega
{
    public class MovimientoInventarioAuditoriaCompletaDto
    {
        public MovimientoInventarioAuditoriaResumenDto Resumen { get; set; } = new();

        public List<MovimientoInventarioAuditoriaDto> Movimientos { get; set; } = new();

        public List<MovimientoInventarioEstadisticaDto> TotalesPorTipo { get; set; } = new();

        public List<ReferenciaMasMovidaDto> ReferenciasMasMovidas { get; set; } = new();

        public List<UsuarioMasActivoDto> UsuariosMasActivos { get; set; } = new();

        // Paginación
        public int Pagina { get; set; }
        public int PageSize { get; set; }
        public int TotalRegistros { get; set; }
        public int TotalPaginas { get; set; }
    }
}