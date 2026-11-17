namespace SistemaFlota.Services.InventarioBodega.EntradasBodega.EntradasInventarioPdf
{
    public interface IEntradasInventarioPdfService
    {
        Task<byte[]> GenerarPdfAsync(
            string? buscar = null,
            DateTime? fechaInicio = null,
            DateTime? fechaFin = null);
    }
}
