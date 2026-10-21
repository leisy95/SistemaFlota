namespace SistemaFlota.Services.Flota.Inspecciones.InspecionPdfGeneral
{
    public interface IInspeccionPdfGeneralService
    {
        Task<byte[]> GenerarPdfGeneralAsync(
            string? buscar = null,
            DateTime? fechaDesde = null,
            DateTime? fechaHasta = null);
    }
}
