using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;

namespace SistemaFlota
{
    public class FlotaChatService : IMensajeriaService
    {
        private readonly HttpClient _http;
        private readonly string _apiKey;
        private readonly string _apiUrl;
        private readonly ILogger<FlotaChatService> _logger;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IConfiguration _config;
        public FlotaChatService(IConfiguration config, ILogger<FlotaChatService> logger, IServiceScopeFactory scopeFactory)
        {
            _http = new HttpClient();
            _apiKey = config["FlotaChat:ApiKey"] ?? throw new InvalidOperationException("Falta configurar FlotaChat:ApiKey");
            _apiUrl = config["FlotaChat:ApiUrl"] ?? "https://apichat.gecobagsci.com";
            _logger = logger;
            _scopeFactory = scopeFactory;
            _config = config;
            _http.DefaultRequestHeaders.Add("X-Api-Key", _apiKey);
        }

        // ── Envío a un solo número: individual si está vinculado, si no, grupo ──
        public async Task EnviarMensajeAsync(string numeroDestino, string mensaje)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                var vinculacion = !string.IsNullOrWhiteSpace(numeroDestino)
                    ? await db.VinculacionesFlotaChat.FirstOrDefaultAsync(v => v.Telefono == numeroDestino)
                    : null;

                if (vinculacion != null)
                {
                    await EnviarMensajeIndividualAsync(vinculacion.FlotaChatUsuarioId, mensaje);
                    return;
                }

                await EnviarAlGrupoCompartidoAsync(mensaje);
                _logger.LogWarning("⚠️ Número {Numero} sin vincular a FlotaChat, se envió al grupo compartido", numeroDestino);
            }
            catch (Exception ex)
            {
                _logger.LogError("❌ Error FlotaChat: {Error}", ex.Message);
            }
        }

        // ── Envío a varios números: individual a cada uno vinculado ──────────
        public async Task EnviarAMultiplesAsync(List<string> numeros, string mensaje)
        {
            await EnviarAMultiplesAsync(numeros, mensaje, "General");
        }

        public async Task EnviarAMultiplesAsync(List<string> numeros, string mensaje, string categoria)
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var sinVincular = new List<string>();
            foreach (var numero in numeros)
            {
                var vinculacion = await db.VinculacionesFlotaChat.FirstOrDefaultAsync(v => v.Telefono == numero);
                if (vinculacion != null)
                    await EnviarMensajeIndividualAsync(vinculacion.FlotaChatUsuarioId, mensaje);
                else
                    sinVincular.Add(numero);
            }
            if (sinVincular.Count > 0)
            {
                _logger.LogWarning("⚠️ {Cantidad} contactos sin vincular, reciben por grupo compartido ({Categoria}): {Numeros}",
                    sinVincular.Count, categoria, string.Join(", ", sinVincular));
                await EnviarAlGrupoCompartidoAsync(mensaje, categoria);
            }
        }
        // ── Helper: mensaje individual por chat directo (UsuarioId) ──────────
        private async Task EnviarMensajeIndividualAsync(int usuarioId, string mensaje)
        {
            try
            {
                var body = new
                {
                    usuarioId,
                    contenido = mensaje,
                    tipo = "notificacion",
                    nombreOrigen = "SistemaFlota"
                };
                var json = JsonSerializer.Serialize(body);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _http.PostAsync($"{_apiUrl}/api/Externo/mensaje", content);

                if (response.IsSuccessStatusCode)
                    _logger.LogInformation("✅ Mensaje individual enviado — usuario FlotaChat {UsuarioId}", usuarioId);
                else
                    _logger.LogWarning("⚠️ FlotaChat respondió: {Status} (usuario {UsuarioId})", response.StatusCode, usuarioId);
            }
            catch (Exception ex)
            {
                _logger.LogError("❌ Error FlotaChat individual: {Error}", ex.Message);
            }
        }

        // ── Helper: fallback al grupo compartido (contactos aún sin vincular) ─
        private async Task EnviarAlGrupoCompartidoAsync(string mensaje, string categoria = "General")
        {
            try
            {
                var grupoId = _config[$"FlotaChat:Grupo{categoria}"] != null
                    ? int.Parse(_config[$"FlotaChat:Grupo{categoria}"]!)
                    : 1; // "Operadores" como respaldo si no hay grupo configurado para esa categoría

                var body = new
                {
                    grupoId,
                    contenido = mensaje,
                    tipo = "notificacion",
                    nombreOrigen = "SistemaFlota"
                };
                var json = JsonSerializer.Serialize(body);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _http.PostAsync($"{_apiUrl}/api/Externo/mensaje", content);

                if (response.IsSuccessStatusCode)
                    _logger.LogInformation("✅ Mensaje enviado al grupo compartido");
                else
                    _logger.LogWarning("⚠️ FlotaChat respondió: {Status} (grupo compartido)", response.StatusCode);
            }
            catch (Exception ex)
            {
                _logger.LogError("❌ Error FlotaChat grupo compartido: {Error}", ex.Message);
            }
        }
    }
}
