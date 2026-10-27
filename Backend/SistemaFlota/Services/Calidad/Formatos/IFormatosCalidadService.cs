using SistemaFlota.DTOs.Calidad.Formatos;
using SistemaFlota.Models;
using SistemaFlota.Models.Calidad;

namespace SistemaFlota.Services.Calidad.Formatos
{
    public interface IFormatosCalidadService
    {
        Task<List<TipoFormatoCalidad>> ObtenerTiposAsync();

        Task<TipoFormatoCalidad?> ObtenerCaracteristicasAsync(
            string codigo);

        Task<List<RegistroFormatoCalidad>> ObtenerRegistrosAsync(
            string codigo,
            DateTime? desde,
            DateTime? hasta,
            string? op);

        Task<RegistroFormatoCalidad> CrearAsync(
            RegistroFormatoCalidadDto dto,
            string usuario,
            string rol,
            int usuarioId);

        Task<RegistroFormatoCalidad?> ActualizarAsync(
            int id,
            RegistroFormatoCalidadDto dto);

        Task<RegistroFormatoCalidad?> LiberarAsync(
            int id,
            LiberarFormatoDto dto,
            string usuario,
            string rol);

        Task<RegistroFormatoCalidad?> ObtenerPorOPAsync(
            string op,
            int tipoFormatoId);

        Task<List<MezclaOrdenItem>> ObtenerMezclaAsync(
            string op);

        Task<bool> EliminarAsync(int id);

        Task<(decimal TotalDesperdicio, int TotalRegistros)>
            ObtenerDesperdicioPorOrdenAsync(
                string op,
                int tipoFormatoId);

        Task<object> ObtenerMejorRendimientoAsync(
            string texto,
            string? maquina);

        Task<bool> GuardarParametrosOperarioAsync(
            int id,
            GuardarParametrosOperarioDto dto);

        Task<List<RegistroParametrosOperario>>
            ObtenerParametrosOperarioAsync(int id);

        Task<(
            bool Exito,
            string? Mensaje,
            List<FormatoCalidadFoto>? Fotos
        )> SubirFotosRondaAsync(
            int id,
            string rondaId,
            List<IFormFile> fotos,
            string usuario);

        Task<List<FormatoCalidadFoto>> ObtenerFotosRondasAsync(
            int id);

        Task<bool> EliminarFotoRondaAsync(
            int id,
            int fotoId);

        Task<RegistroFormatoCalidad?> FijarMejorRendimientoAsync(
            int id);

        Task<RegistroFormatoCalidad?> QuitarFijadoAsync(
            int id);
    }
}
