namespace SistemaFlota.Services.Consecutivos
{
    public interface IConsecutivoService
    {
        Task<string> GenerarAsync(string modulo);
    }
}