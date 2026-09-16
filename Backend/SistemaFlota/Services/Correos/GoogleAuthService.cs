using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Gmail.v1;
using Google.Apis.Services;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SistemaFlota.Configuracion;
using SistemaFlota.Models.Correos;
using SistemaFlota.Services.Correos;
using System.Text;

namespace SistemaFlota.Services.Correos;

public class GoogleAuthService : IGoogleAuthService
{
    private const string GmailSendScope =
        "https://www.googleapis.com/auth/gmail.send";

    private readonly GoogleOAuthSettings _settings;
    private readonly AppDbContext _context;
    private readonly IDataProtector _protector;

    public GoogleAuthService(
        IOptions<GoogleOAuthSettings> options,
        AppDbContext context,
        IDataProtectionProvider dataProtectionProvider)
    {
        _settings = options.Value;
        _context = context;

        _protector = dataProtectionProvider.CreateProtector(
            "SistemaFlota.GoogleOAuth.State.v1");
    }

    public Task<string> GenerarUrlAutorizacionAsync(int usuarioId)
    {
        if (usuarioId <= 0)
            throw new ArgumentException(
                "El usuario no es válido.");

        if (string.IsNullOrWhiteSpace(_settings.ClientId) ||
            string.IsNullOrWhiteSpace(_settings.ClientSecret) ||
            string.IsNullOrWhiteSpace(_settings.RedirectUri))
        {
            throw new InvalidOperationException(
                "La configuración de Google OAuth no está completa.");
        }

        // Estado protegido que identifica al usuario
        var datos = $"{usuarioId}|{Guid.NewGuid():N}";
        var state = _protector.Protect(datos);

        var parametros = new Dictionary<string, string>
        {
            ["client_id"] = _settings.ClientId,
            ["redirect_uri"] = _settings.RedirectUri,
            ["response_type"] = "code",
            ["scope"] = GmailSendScope,
            ["access_type"] = "offline",
            ["prompt"] = "consent",
            ["state"] = state
        };

        var query = string.Join(
            "&",
            parametros.Select(x =>
                $"{Uri.EscapeDataString(x.Key)}=" +
                $"{Uri.EscapeDataString(x.Value)}"));

        var url =
            "https://accounts.google.com/o/oauth2/v2/auth?" +
            query;

        return Task.FromResult(url);
    }

    public async Task ProcesarCallbackAsync(
        string code,
        string state)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException(
                "Google no devolvió el código de autorización.");

        if (string.IsNullOrWhiteSpace(state))
            throw new ArgumentException(
                "No se recibió el estado de autorización.");

        string datos;

        try
        {
            datos = _protector.Unprotect(state);
        }
        catch
        {
            throw new InvalidOperationException(
                "El estado de autorización de Google no es válido o ha expirado.");
        }

        var partes = datos.Split('|');

        if (partes.Length != 2 ||
            !int.TryParse(partes[0], out var usuarioId) ||
            usuarioId <= 0)
        {
            throw new InvalidOperationException(
                "No se pudo identificar al usuario de Google OAuth.");
        }

        var flow = new GoogleAuthorizationCodeFlow(
            new GoogleAuthorizationCodeFlow.Initializer
            {
                ClientSecrets = new ClientSecrets
                {
                    ClientId = _settings.ClientId,
                    ClientSecret = _settings.ClientSecret
                },
                Scopes = new[]
                {
                    GmailSendScope
                }
            });

        var token = await flow.ExchangeCodeForTokenAsync(
            usuarioId.ToString(),
            code,
            _settings.RedirectUri,
            CancellationToken.None);

        if (string.IsNullOrWhiteSpace(token.RefreshToken))
        {
            throw new InvalidOperationException(
                "Google no devolvió un Refresh Token. " +
                "Vuelve a autorizar la cuenta.");
        }

        var credential = new UserCredential(
            flow,
            usuarioId.ToString(),
            token);

        var gmail = new GmailService(
            new BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName = "SistemaFlota"
            });

        var perfil = await gmail.Users
            .GetProfile("me")
            .ExecuteAsync();

        if (string.IsNullOrWhiteSpace(perfil.EmailAddress))
        {
            throw new InvalidOperationException(
                "No se pudo obtener el correo de Gmail autorizado.");
        }

        var correo = await _context.CorreosAutorizados
            .FirstOrDefaultAsync(x =>
                x.UsuarioId == usuarioId &&
                x.Proveedor == "Google");

        if (correo == null)
        {
            correo = new CorreoAutorizado
            {
                UsuarioId = usuarioId,
                Proveedor = "Google",
                Email = perfil.EmailAddress,
                RefreshToken = token.RefreshToken,
                FechaAutorizacion = DateTime.UtcNow,
                Activo = true
            };

            _context.CorreosAutorizados.Add(correo);
        }
        else
        {
            correo.Email = perfil.EmailAddress;
            correo.RefreshToken = token.RefreshToken;
            correo.FechaAutorizacion = DateTime.UtcNow;
            correo.Activo = true;
        }

        await _context.SaveChangesAsync();
    }
}