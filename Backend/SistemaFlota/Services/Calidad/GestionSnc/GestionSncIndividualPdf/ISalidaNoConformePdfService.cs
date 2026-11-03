namespace SistemaFlota.Services.Calidad.GestionSnc.GestionSncIndividualPdf
{
    public interface ISalidaNoConformePdfService
    {
        Task<byte[]> GenerarPdfAsync(int registroId);
    }
}

