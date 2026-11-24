namespace SistemaFlota.Services.RecursosHumanos.RecursosHumanosDetallePdf
{
    public interface ISeguimientoRrhhDetallePdfService
    {
        Task<byte[]> GenerarPdfAsync(int id);
    }
}
