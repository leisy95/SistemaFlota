namespace SistemaFlota.Services.ComprasNoFormalizadas.RecepcionMercancia
{
    public interface IRecepcionMercanciaNoFormalizadaPdfService
    {
        Task<byte[]> GenerarPdfAsync(int idRecepcion);
    }
}
