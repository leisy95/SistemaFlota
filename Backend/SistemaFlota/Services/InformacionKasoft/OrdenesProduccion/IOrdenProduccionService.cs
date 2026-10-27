
using SistemaFlota.Models;

namespace SistemaFlota.Services.InformacionKasoft.OrdenesProduccion
{
    public interface IOrdenProduccionService
    {
        Task<OrdenProduccionExterna?> BuscarPorNumeroAsync(
            string numeroOP);

        Task<decimal> ObtenerCantidadRealAsync(
            string numeroOP);

        Task<List<string>> BuscarReferenciasPorDescripcionAsync(
            string texto);

        Task<Dictionary<string, string>> ObtenerDescripcionesReferenciasAsync(
             IEnumerable<string> referencias);

        Task<List<OrdenProduccionExterna>> ImportarDesdeArchivo(
            Stream archivo,
            string nombreArchivo);
    }
}
