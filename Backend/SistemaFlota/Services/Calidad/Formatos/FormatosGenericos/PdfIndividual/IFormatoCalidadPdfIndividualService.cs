namespace SistemaFlota.Services.Calidad.Formatos.FormatosGenericos.PdfIndividual
{
    public interface IFormatoCalidadPdfIndividualService
    {
        Task<byte[]> GenerarPdfRegistroAsync(int registroId);
    }
}
