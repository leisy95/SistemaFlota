using System.Text;
using System.Text.Json;

namespace SistemaFlota
{
    public class FlotaChatService : ITwilioService
    {
        private readonly HttpClient _http;
        private readonly string _apiKey;
        private readonly string _apiUrl;
        private readonly ILogger<FlotaChatService> _logger;

        public FlotaChatService(IConfiguration config, ILogger<FlotaChatService> logger)
        {
            _http = new HttpClient();
            _apiKey = config["FlotaChat:ApiKey"] ?? "FlotaChat_API_Key_2026_Seguro_XYZ789";
            _apiUrl = config["FlotaChat:ApiUrl"] ?? "https://apichat.gecobagsci.com";
            _logger = logger;
            _http.DefaultRequestHeaders.Add("X-Api-Key", _apiKey);
        }

        public async Task EnviarMensajeAsync(string numeroDestino, string mensaje)
        {
            try
            {
                var body = new {
                    grupoId = 1,
                    contenido = mensaje,
                    tipo = "notificacion",
                    nombreOrigen = "SistemaFlota"
                };
                var json = JsonSerializer.Serialize(body);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _http.PostAsync($"{_apiUrl}/api/Externo/mensaje", content);
                if (response.IsSuccessStatusCode)
                    _logger.LogInformation("✅ Mensaje enviado a FlotaChat");
                else
                    _logger.LogWarning("⚠️ FlotaChat respondio: {Status}", response.StatusCode);
            }
            catch (Exception ex)
            {
                _logger.LogError("❌ Error FlotaChat: {Error}", ex.Message);
            }
        }

        public async Task EnviarAMultiplesAsync(List<string> numeros, string mensaje)
        {
            await EnviarMensajeAsync("", mensaje);
        }
    }
}
