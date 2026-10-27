
using SistemaFlota.DTOs.InformacionKasoft;

namespace SistemaFlota.Services.InformacionKasoft
{
    public interface IInformacionKasoftService
    {
        Task<List<StockReferenciaDto>> ObtenerStocksPorReferenciaAsync();

        Task<List<EntradaDto>> ObtenerEntradasAsync();

        Task<List<MezclaPorOrdenDto>> ObtenerMezclaPorOrdenAsync();

        Task<List<MateriasPrimasDto>> ObtenerMateriasPrimasAsync();

        Task<List<OrdenesProduccionDto>> ObtenerOrdenesProduccionAsync();

        Task<List<ReferenciasDto>> ObtenerReferenciasAsync();

        Task<List<RegistroProduccionDto>> ObtenerRegistroProduccionAsync();
    }
}
