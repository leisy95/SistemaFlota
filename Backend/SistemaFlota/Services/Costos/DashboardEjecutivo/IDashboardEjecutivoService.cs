
using SistemaFlota.DTOs.Costos.DashboardEjecutivo;

namespace SistemaFlota.Services.Costos.DashboardEjecutivo
{
    public interface IDashboardEjecutivoService
    {
        Task<DashboardEjecutivoDto> ObtenerDashboardAsync(
            int pagina = 1,
            int porPagina = 5,
            DateTime? fechaDesde = null,
            DateTime? fechaHasta = null,
            string? proveedor = null,
            string? estado = null,
            string? categoria = null,
            string? densidad = null
        );

        Task<FiltrosDashboardEjecutivoDto> ObtenerFiltrosAsync();
    }
}
