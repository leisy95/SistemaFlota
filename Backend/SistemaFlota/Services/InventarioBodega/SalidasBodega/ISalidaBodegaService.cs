
using SistemaFlota.DTOs.InventarioBodega.SalidasBodega;

namespace SistemaFlota.Services.InventarioBodega.SalidasBodega
{
    public interface ISalidaBodegaService
    {
        Task<SalidaBodegaActualDto> ObtenerSalidaActualAsync();
        Task<RespuestaSalidaBodegaDto> CrearSalidaAsync();
        Task<RespuestaSalidaBodegaDto> AgregarItemAsync(
            int idSalida,
            int idInventarioBodegaItem
        );
        Task<RespuestaSalidaBodegaDto> AgregarItemPorReferenciaAsync(
            int idSalida,
            string referencia
        );
        Task<RespuestaSalidaBodegaDto> QuitarItemAsync(
            int idSalida,
            int idInventarioBodegaItem
        );
        Task<RespuestaSalidaBodegaDto> ConfirmarSalidaAsync(
            int idSalida,
            ConfirmarSalidaBodegaDto dto
        );
        Task<RespuestaSalidaBodegaDto> CancelarSalidaAsync(
            int idSalida
        );
    }
}
