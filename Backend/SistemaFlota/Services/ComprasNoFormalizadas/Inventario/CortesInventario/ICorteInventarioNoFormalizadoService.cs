using SistemaFlota.DTOs.ComprasNoFormalizadas.Inventario.CortesInventario;

namespace SistemaFlota.Services.ComprasNoFormalizadas.Inventario.CortesInventario
{
    public interface ICorteInventarioNoFormalizadoService
    {
        Task<List<CorteInventarioNoFormalizadoDto>> ObtenerCorteAsync();

        Task GuardarCorteAsync(CrearCorteInventarioNoFormalizadoDto dto);

        Task<List<HistorialCorteInventarioNoFormalizadoDto>> ObtenerHistorialAsync();

        Task<HistorialCorteDetalleNoFormalizadoDto?> ObtenerDetalleAsync(int id);
    }
}
