namespace SistemaFlota.Services.Costos.OrdenCompra
{
    public interface IOrdenCompraPdfService
    {
        Task<byte[]> GenerarPdfAsync(int idOrden);
    }
}