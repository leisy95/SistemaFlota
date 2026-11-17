using SistemaFlota.DTOs.InventarioBodega.DashboardBodega;

namespace SistemaFlota.Services.InventarioBodega.DashboardBodega.MovimientosInventarioPdf
{
    public interface IMovimientosInventarioPdfService
    {
        Task<byte[]> GenerarPdfAsync(
            MovimientoInventarioFiltroDto filtros
        );
    }
}
