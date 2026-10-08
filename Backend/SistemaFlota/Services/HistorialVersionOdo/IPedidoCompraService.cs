using SistemaFlota.DTOs.HistorialVersionOdo;

namespace SistemaFlota.Services.HistorialVersionOdo
{
    public interface IPedidoCompraService
    {
        Task<PaginacionDto<PedidoCompraDto>> ObtenerAsync(
            int pagina = 1,
            int porPagina = 20,
            string? buscar = null,
            string? prioridad = null,
            string? estado = null,
            DateTime? fechaDesde = null,
            DateTime? fechaHasta = null);

        Task<PedidoCompraDto?> ObtenerPorIdAsync(int id);
    }
}
