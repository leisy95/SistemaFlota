using SistemaFlota.DTOs.Costos.Inventario.CortesInventario;

namespace SistemaFlota.Services.Costos.Inventario.CortesInventario
{
    public interface ICorteInventarioService
    {
        Task<List<CorteInventarioDto>> ObtenerCorteAsync();
        Task<FiltrosCorteInventarioDto> ObtenerFiltrosAsync();
        Task GuardarCorteAsync(CrearCorteInventarioDto dto);
        Task<List<HistorialCorteInventarioDto>> ObtenerHistorialAsync();
        Task<HistorialCorteDetalleDto?> ObtenerDetalleAsync(int id);
    }
}