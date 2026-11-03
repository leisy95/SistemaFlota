using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using SistemaFlota.DTOs;

namespace SistemaFlota
{
    // Envía los avisos de SistemaFlota a FlotaChat por la integración con token.
    public class FlotaChatService : IMensajeriaService, IFlotaChatDirectorio, IFlotaChatAvisos
    {
        public const string NombreCliente = "FlotaChat";
        private const int Intentos = 3;

        private readonly IHttpClientFactory _http;
        private readonly ILogger<FlotaChatService> _logger;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IConfiguration _config;
        private readonly string _token;
        private readonly int? _redirigirTodoA;

        private record AvisoAceptado(int MensajeId, bool Duplicado);

        public FlotaChatService(IHttpClientFactory http, IConfiguration config, ILogger<FlotaChatService> logger,
                                IServiceScopeFactory scopeFactory)
        {
            _http = http;
            _config = config;
            _logger = logger;
            _scopeFactory = scopeFactory;
            _token = config["FlotaChat:Token"] ?? throw new InvalidOperationException("Falta configurar FlotaChat:Token");

            // Solo para pruebas: todo aviso va a este canal (nunca a personas reales).
            _redirigirTodoA = int.TryParse(config["FlotaChat:RedirigirTodoA"], out var canal) ? canal : null;
            if (_redirigirTodoA.HasValue)
                _logger.LogWarning("FlotaChat en modo pruebas: todos los avisos van al canal {Canal}", _redirigirTodoA);
        }

        // ── IMensajeriaService ─────────────────────────────────────────────

        public Task EnviarMensajeAsync(string numeroDestino, string mensaje) =>
            EnviarMensajeAsync(numeroDestino, mensaje, "General");

        public async Task EnviarMensajeAsync(string numeroDestino, string mensaje, string categoria)
        {
            try
            {
                var usuarioId = await BuscarUsuarioVinculadoAsync(numeroDestino);
                if (usuarioId.HasValue)
                {
                    await EnviarAvisoAsync("usuario", usuarioId.Value, mensaje, categoria);
                    return;
                }

                _logger.LogWarning("Número {Numero} sin vincular a FlotaChat; se envía al grupo de {Categoria}", numeroDestino, categoria);
                await PublicarEnGrupoAsync(categoria, mensaje);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error enviando aviso a FlotaChat");
            }
        }

        public Task EnviarAMultiplesAsync(List<string> numeros, string mensaje) =>
            EnviarAMultiplesAsync(numeros, mensaje, "General");

        public async Task EnviarAMultiplesAsync(List<string> numeros, string mensaje, string categoria)
        {
            try
            {
                var sinVincular = new List<string>();
                foreach (var numero in numeros)
                {
                    var usuarioId = await BuscarUsuarioVinculadoAsync(numero);
                    if (usuarioId.HasValue)
                        await EnviarAvisoAsync("usuario", usuarioId.Value, mensaje, categoria);
                    else
                        sinVincular.Add(numero);
                }

                if (sinVincular.Count > 0)
                {
                    _logger.LogWarning("{Cantidad} contactos sin vincular reciben por el grupo de {Categoria}: {Numeros}",
                        sinVincular.Count, categoria, string.Join(", ", sinVincular));
                    await PublicarEnGrupoAsync(categoria, mensaje);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error enviando avisos múltiples a FlotaChat");
            }
        }

        // ── IFlotaChatAvisos ───────────────────────────────────────────────

        public Task<int?> PublicarEnGrupoAsync(string categoria, string texto)
        {
            var grupoId = int.TryParse(_config[$"FlotaChat:Grupo{categoria}"], out var id) ? id : 1;
            return EnviarAvisoAsync("grupo", grupoId, texto, categoria);
        }

        public Task<int?> EnviarAUsuarioAsync(int flotaChatUsuarioId, string texto, string categoria) =>
            EnviarAvisoAsync("usuario", flotaChatUsuarioId, texto, categoria);

        // ── IFlotaChatDirectorio ───────────────────────────────────────────

        public async Task<List<UsuarioFlotaChatDto>?> ObtenerUsuariosAsync()
        {
            try
            {
                using var peticion = new HttpRequestMessage(HttpMethod.Get, "api/integracion/usuarios");
                peticion.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _token);

                using var respuesta = await _http.CreateClient(NombreCliente).SendAsync(peticion);
                if (!respuesta.IsSuccessStatusCode)
                {
                    _logger.LogWarning("FlotaChat respondió {Estado} al pedir usuarios", (int)respuesta.StatusCode);
                    return null;
                }
                return await respuesta.Content.ReadFromJsonAsync<List<UsuarioFlotaChatDto>>() ?? new();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "No se pudo obtener la lista de usuarios de FlotaChat");
                return null;
            }
        }

        // ── Internos ───────────────────────────────────────────────────────

        private async Task<int?> BuscarUsuarioVinculadoAsync(string? numero)
        {
            if (string.IsNullOrWhiteSpace(numero)) return null;

            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            return await db.VinculacionesFlotaChat
                .Where(v => v.Telefono == numero)
                .Select(v => (int?)v.FlotaChatUsuarioId)
                .FirstOrDefaultAsync();
        }

        
        private async Task<int?> EnviarAvisoAsync(string tipoDestino, int destinoId, string mensaje, string categoria)
        {
            var texto = mensaje;
            if (_redirigirTodoA.HasValue)
            {
                texto = $"[Prueba: iba para {tipoDestino} {destinoId}]\n{mensaje}";
                tipoDestino = "canal";
                destinoId = _redirigirTodoA.Value;
            }

            var aviso = new
            {
                idExterno = Guid.NewGuid().ToString("N"),
                modulo = string.IsNullOrWhiteSpace(categoria) ? "General" : categoria,
                prioridad = "normal",
                destino = new { tipo = tipoDestino, id = destinoId },
                texto,
                remitente = "SistemaFlota"
            };

            for (var intento = 1; intento <= Intentos; intento++)
            {
                try
                {
                    using var peticion = new HttpRequestMessage(HttpMethod.Post, "api/integracion/avisos")
                    {
                        Content = JsonContent.Create(aviso)
                    };
                    peticion.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _token);

                    using var respuesta = await _http.CreateClient(NombreCliente).SendAsync(peticion);
                    if (respuesta.IsSuccessStatusCode)
                    {
                        var aceptado = await respuesta.Content.ReadFromJsonAsync<AvisoAceptado>();
                        _logger.LogInformation("Aviso {IdExterno} enviado a FlotaChat ({Tipo} {Destino}), mensaje {MensajeId}",
                            aviso.idExterno, tipoDestino, destinoId, aceptado?.MensajeId);
                        return aceptado?.MensajeId;
                    }

                    var codigo = (int)respuesta.StatusCode;
                    if (codigo >= 400 && codigo < 500 && respuesta.StatusCode != HttpStatusCode.TooManyRequests)
                    {
                        var detalle = await respuesta.Content.ReadAsStringAsync();
                        _logger.LogError("FlotaChat rechazó el aviso {IdExterno} ({Codigo}): {Detalle}", aviso.idExterno, codigo, detalle);
                        return null;
                    }

                    _logger.LogWarning("FlotaChat respondió {Codigo} al aviso {IdExterno} (intento {Intento} de {Total})",
                        codigo, aviso.idExterno, intento, Intentos);
                }
                catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
                {
                    _logger.LogWarning("No se pudo contactar FlotaChat para el aviso {IdExterno} (intento {Intento} de {Total}): {Error}",
                        aviso.idExterno, intento, Intentos, ex.Message);
                }

                if (intento < Intentos) await Task.Delay(TimeSpan.FromSeconds(2 * intento));
            }

            _logger.LogError("El aviso {IdExterno} no se pudo entregar a FlotaChat después de {Total} intentos", aviso.idExterno, Intentos);
            return null;
        }
    }
}