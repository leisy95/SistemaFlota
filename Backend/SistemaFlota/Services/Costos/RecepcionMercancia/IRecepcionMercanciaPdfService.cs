namespace SistemaFlota.Services.Pdf.RecepcionMercancia;

public interface IRecepcionMercanciaPdfService
{
    Task<byte[]> GenerarPdfAsync(int idRecepcion);
}