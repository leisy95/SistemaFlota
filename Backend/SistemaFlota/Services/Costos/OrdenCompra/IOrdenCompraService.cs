using SistemaFlota.DTOs.Costos.OrdenCompra;

namespace SistemaFlota.Services.Costos.OrdenCompra
{  public interface IOrdenCompraService
    {
        Task<OrdenCompraPaginadoDto> ObtenerAsync(
            string? search,
            string? estado,
            int? proveedorId,
            string? formaPago,
            DateTime? fechaInicio,
            DateTime? fechaFin,
            int page,
            int pageSize
        );

        Task<OrdenCompraDto?> ObtenerPorIdAsync(int id);

        Task<OrdenCompraDto> CrearAsync(CrearOrdenCompraDto dto);

        Task<bool> ActualizarAsync(int id, ActualizarOrdenCompraDto dto);

        Task<bool> EliminarAsync(int id);

        Task<FiltrosOrdenCompraDto> ObtenerFiltrosAsync();
    }
}
