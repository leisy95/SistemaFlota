namespace SistemaFlota.Services.ComprasNoFormalizadas.OrdenesCompras
{
    public interface IOrdenCompraNoFormalizadaPdfService
    {
        Task<byte[]> GenerarPdfAsync(int idOrden);
    }
}
