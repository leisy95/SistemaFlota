namespace SistemaFlota.Services.Costos.Inventario.CortesInventario
{
    public interface ICorteInventarioPdfService
    {
        Task<byte[]> GenerarPdfAsync();
    }
}