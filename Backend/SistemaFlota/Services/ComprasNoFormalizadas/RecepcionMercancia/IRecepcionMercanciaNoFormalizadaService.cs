using SistemaFlota.DTOs.ComprasNoFormalizadas.RecepcionMercancias;

namespace SistemaFlota.Services.ComprasNoFormalizadas.RecepcionMercancia
{
    public interface IRecepcionMercanciaNoFormalizadaService
    {
        Task<RecepcionMercanciaNoFormalizadaPaginadoDto> ObtenerAsync(
            string? search,
            DateTime? fechaInicio,
            DateTime? fechaFin,
            int? proveedorId,
            int page,
            int pageSize
        );

        Task<RecepcionMercanciaNoFormalizadaDto?> ObtenerPorIdAsync(int id);

        Task<RecepcionFormularioNoFormalizadaDto?> ObtenerFormularioAsync(
            int ordenCompraId);

        Task<RecepcionMercanciaNoFormalizadaDto> CrearAsync(
            CrearRecepcionMercanciaNoFormalizadaDto dto);

        Task ConfirmarRecepcionAsync(int id);

        Task<bool> ActualizarAsync(
            int id,
            ActualizarRecepcionMercanciaNoFormalizadaDto dto);

        Task<bool> EliminarAsync(int id);

        Task<FiltrosRecepcionMercanciaNoFormalizadaDto>
            ObtenerFiltrosAsync();
    }
}
