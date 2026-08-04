using SistemaFlota.Models;

namespace SistemaFlota
{
    public interface IProveedorOrdenesProduccion
    {
        Task<OrdenProduccionExterna?> BuscarPorNumero(string numeroOP);
        Task<List<OrdenProduccionExterna>> ImportarDesdeArchivo(Stream archivo, string nombreArchivo);
    }
}