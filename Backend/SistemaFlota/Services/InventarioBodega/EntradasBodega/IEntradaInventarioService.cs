
using SistemaFlota.DTOs.InventarioBodega.Devoluciones;
using SistemaFlota.DTOs.InventarioBodega.EntradasBodega;
using SistemaFlota.DTOs.InventarioBodega.Inventario;

namespace SistemaFlota.Services.InventarioBodega.EntradasBodega
{
    public interface IEntradaInventarioService
    {
        Task<EntradasBodegaPaginadasDto> FiltrarEntradasAsync(
            FiltroEntradaBodegaDto filtros);
        Task<List<MostrarEntradaDto>> ObtenerEntradasAsync(
            string? estado = null);

        Task<IngresarPesoResponseDto> IngresarPesoAsync(
            IngresarPesoDto request,
            string usuario);

        Task<string> RegistrarDevolucionAsync(
            DevolucionEntradaDto request,
            string usuario);

        Task<string> AjusteGeneralAsync(
            AjusteGeneralEntradaDto request,
            string usuario);
    }
}
