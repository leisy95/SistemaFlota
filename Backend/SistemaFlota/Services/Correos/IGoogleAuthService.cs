namespace SistemaFlota.Services.Correos;

public interface IGoogleAuthService
{
    Task<string> GenerarUrlAutorizacionAsync(int usuarioId);

    Task ProcesarCallbackAsync(
        string code,
        string state);
}