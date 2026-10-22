using SistemaFlota.DTOs.ControlEnvios.CostosFletes;

namespace SistemaFlota.Services.ControlEnvios.CostosFlete
{
    public interface ICostosFletesService
    {
        Task<List<CosteFlete>> ObtenerAsync(
            string? desde,
            string? hasta,
            string? conductor,
            string? estado,
            string? ciudad,
            string? placa);

        Task<CosteFlete> CrearAsync(
            CostoFleteDto dto,
            string usuario,
            string rol);

        Task<CosteFlete?> EditarAsync(
            int id,
            CostoFleteDto dto);

        Task<CosteFlete?> VerificarAsync(
            int id,
            VerificarDto dto,
            string usuario,
            string rol);

        Task<bool> EliminarAsync(int id);
    }
}
