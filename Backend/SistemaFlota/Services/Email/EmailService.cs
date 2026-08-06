using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using SistemaFlota.Configuracion;

namespace SistemaFlota.Services.Email;

public class EmailService : IEmailService
{
    private readonly EmailSettings _settings;

    public EmailService(IOptions<EmailSettings> options)
    {
        _settings = options.Value;
    }

    public async Task EnviarAsync(
        string para,
        string asunto,
        string html)
    {
        await EnviarAsync(
            para,
            asunto,
            html,
            null,
            null);
    }

    public async Task EnviarAsync(
        string para,
        string asunto,
        string html,
        byte[]? archivo,
        string? nombreArchivo)
    {
        var mensaje = new MimeMessage();

        mensaje.From.Add(
            new MailboxAddress(
                _settings.DisplayName,
                _settings.From));

        mensaje.To.Add(
            MailboxAddress.Parse(para));

        mensaje.Subject = asunto;

        var builder = new BodyBuilder
        {
            HtmlBody = html
        };

        if (archivo != null)
        {
            builder.Attachments.Add(
                nombreArchivo!,
                archivo,
                ContentType.Parse("application/pdf"));
        }

        mensaje.Body = builder.ToMessageBody();

        using var smtp = new SmtpClient();

        await smtp.ConnectAsync(
            _settings.Host,
            _settings.Port,
            _settings.UseSSL
                ? SecureSocketOptions.SslOnConnect
                : SecureSocketOptions.StartTls);

        await smtp.AuthenticateAsync(
            _settings.UserName,
            _settings.Password);

        await smtp.SendAsync(mensaje);

        await smtp.DisconnectAsync(true);
    }
}