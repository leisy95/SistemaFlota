using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace SistemaFlota
{
    public interface ITwilioService
    {
        Task EnviarMensajeAsync(string numeroDestino, string mensaje);
        Task EnviarAMultiplesAsync(List<string> numeros, string mensaje);
    }

    public class TwilioService : ITwilioService
    {
        private readonly string _accountSid;
        private readonly string _authToken;
        private readonly string _fromNumber;
        private readonly ILogger<TwilioService> _logger;

        public TwilioService(IConfiguration config, ILogger<TwilioService> logger)
        {
            _accountSid = config["Twilio:AccountSid"]!;
            _authToken = config["Twilio:AuthToken"]!;
            _fromNumber = config["Twilio:FromNumber"]!;
            _logger = logger;
            TwilioClient.Init(_accountSid, _authToken);
        }

        public async Task EnviarMensajeAsync(string numeroDestino, string mensaje)
        {
            try
            {
                var numero = LimpiarNumero(numeroDestino);
                var message = await MessageResource.CreateAsync(
                    from: new PhoneNumber(_fromNumber),
                    to: new PhoneNumber($"whatsapp:{numero}"),
                    body: mensaje
                );
                _logger.LogInformation("✅ WhatsApp enviado a {Numero} — SID: {Sid}", numero, message.Sid);
            }
            catch (Exception ex)
            {
                _logger.LogError("❌ Error WhatsApp a {Numero}: {Error}", numeroDestino, ex.Message);
            }
        }

        public async Task EnviarAMultiplesAsync(List<string> numeros, string mensaje)
        {
            var tareas = numeros
                .Where(n => !string.IsNullOrWhiteSpace(n))
                .Distinct()
                .Select(n => EnviarMensajeAsync(n, mensaje));
            await Task.WhenAll(tareas);
        }

        private static string LimpiarNumero(string numero)
        {
            numero = numero.Trim().Replace(" ", "").Replace("-", "");
            if (!numero.StartsWith("+"))
                numero = "+57" + numero;
            return numero;
        }
    }
}