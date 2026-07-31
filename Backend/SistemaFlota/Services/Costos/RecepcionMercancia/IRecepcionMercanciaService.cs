using SistemaFlota.DTOs.Costos.RecepcionMercancia;

namespace SistemaFlota.Services.Costos.RecepcionMercancia
{
    public interface IRecepcionMercanciaService
    {
        Task<RecepcionMercanciaPaginadoDto> ObtenerAsync(
            string? search,
            DateTime? fechaInicio,
            DateTime? fechaFin,
            int? proveedorId,
            int page,
            int pageSize
        );

        Task<RecepcionMercanciaDto?> ObtenerPorIdAsync(int id);

        Task<RecepcionFormularioDto?> ObtenerFormularioAsync(int ordenCompraId);

        Task<RecepcionMercanciaDto> CrearAsync(CrearRecepcionMercanciaDto dto);

        Task<bool> ActualizarAsync(int id, ActualizarRecepcionMercanciaDto dto);

        Task<bool> EliminarAsync(int id);

        Task<FiltrosRecepcionMercanciaDto> ObtenerFiltrosAsync();
    }
}