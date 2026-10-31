using Microsoft.AspNetCore.Http;
using SistemaFlota.DTOs.Calidad.FormatoFGC008;
using SistemaFlota.Models.Calidad.FormatoFGC008;

namespace SistemaFlota.Services.Calidad.FormatoFGC008
{
    public interface IFormatoFGC008Service
    {
        Task<List<Models.Calidad.FormatoFGC008.FormatoFGC008>> ObtenerTodosAsync(
            DateTime? desde,
            DateTime? hasta,
            string? op);

        Task<FormatoFGC008PaginadoDto> ObtenerPaginadoAsync(
            DateTime? desde,
            DateTime? hasta,
            string? op,
            int pagina,
            int porPagina);

        Task<FormatoFGC008ResumenDto> ObtenerResumenAsync(
            DateTime? desde,
            DateTime? hasta,
            string? op);

        Task<List<Models.Calidad.FormatoFGC008.FormatoFGC008>> ObtenerPorOPAsync(
            string op);

        Task<Models.Calidad.FormatoFGC008.FormatoFGC008?> ObtenerPorIdAsync(int id);

        Task<(Models.Calidad.FormatoFGC008.FormatoFGC008? Registro, bool Duplicado)> CrearAsync(
            FormatoFGC008Dto dto,
            string usuario,
            IFormFile? foto);

        Task<bool> ActualizarAsync(
            int id,
            FormatoFGC008Dto dto,
            string usuario,
            IFormFile? foto);

        Task<bool> EliminarAsync(int id);

        Task<(
            bool Exito,
            string? Mensaje,
            List<FormatoFGC008Foto>? Fotos
        )> SubirFotosAsync(
            int id,
            List<IFormFile> fotos,
            string usuario);

        Task<bool> EliminarFotoAsync(int id, int fotoId);
    }
}