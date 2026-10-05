using SistemaFlota.DTOs.ComprasNoFormalizadas.OrdenesCompras;

namespace SistemaFlota.Services.ComprasNoFormalizadas.OrdenesCompras
{
    public interface IOrdenCompraNoFormalizadaService
    {
        Task<OrdenCompraNoFormalizadaPaginadoDto> ObtenerAsync(
            string? search,
            string? estado,
            int? proveedorNoFormalizadoId,
            string? formaPago,
            DateTime? fechaInicio,
            DateTime? fechaFin,
            int page,
            int pageSize);

        Task<OrdenCompraNoFormalizadaDto?> ObtenerPorIdAsync(int id);

        Task<OrdenCompraNoFormalizadaDto> CrearAsync(CrearOrdenCompraNoFormalizadaDto dto);

        Task<bool> ActualizarAsync(int id, ActualizarOrdenCompraNoFormalizadaDto dto);

        Task<bool> EliminarAsync(int id);

        Task<FiltrosOrdenCompraNoFormalizadaDto> ObtenerFiltrosAsync();

        Task<bool> EnviarPorCorreoAsync(int id);
    }
}
