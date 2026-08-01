namespace SistemaFlota.Services.Email;

public interface IEmailService
{
    Task EnviarAsync(
        string para,
        string asunto,
        string html);

    Task EnviarAsync(
        string para,
        string asunto,
        string html,
        byte[] archivo,
        string nombreArchivo);
}