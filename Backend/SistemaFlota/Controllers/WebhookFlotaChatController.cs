using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaFlota.DTOs;

namespace SistemaFlota.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [AllowAnonymous]
    public class WebhookFlotaChatController : ControllerBase
    {
        private static readonly TimeSpan Tolerancia = TimeSpan.FromMinutes(5);
        private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

        private readonly AppDbContext _context;
        private readonly IConfiguration _config;
        private readonly BotConductorService _bot;
        private readonly ConfirmacionPedidosService _pedidos;
        private readonly ILogger<WebhookFlotaChatController> _logger;

        public WebhookFlotaChatController(AppDbContext context, IConfiguration config, BotConductorService bot,
                                          ConfirmacionPedidosService pedidos, ILogger<WebhookFlotaChatController> logger)
        {
            _context = context;
            _config = config;
            _bot = bot;
            _pedidos = pedidos;
            _logger = logger;
        }

        // Webhook nuevo, firmado.
        [HttpPost("evento")]
        public async Task<IActionResult> Evento()
        {
            var secreto = _config["FlotaChat:WebhookFirmaSecreto"];
            if (string.IsNullOrEmpty(secreto))
            {
                _logger.LogError("Falta configurar FlotaChat:WebhookFirmaSecreto");
                return StatusCode(503, new { mensaje = "Webhook no configurado" });
            }

            using var lector = new StreamReader(Request.Body, Encoding.UTF8);
            var cuerpo = await lector.ReadToEndAsync();

            if (!FirmaValida(secreto, cuerpo))
            {
                _logger.LogWarning("Evento de FlotaChat con firma inválida o vencida");
                return Unauthorized(new { mensaje = "Firma inválida" });
            }

            EventoFlotaChatDto? evento;
            try { evento = JsonSerializer.Deserialize<EventoFlotaChatDto>(cuerpo, Json); }
            catch (JsonException) { return BadRequest(new { mensaje = "Evento con formato inválido" }); }

            if (evento == null || string.IsNullOrWhiteSpace(evento.IdEvento) || evento.IdEvento.Length > 64)
                return BadRequest(new { mensaje = "Evento sin identificador" });

            // Repetido: mismo evento o mismo mensaje de FlotaChat.
            var idEvento = evento.IdEvento;
            var mensajeId = evento.MensajeId;
            var repetido = await _context.RespuestasFlotaChat
                .AnyAsync(r => r.IdEvento == idEvento || (mensajeId > 0 && r.MensajeId == mensajeId));
            if (repetido)
                return Ok(new { mensaje = "Evento ya procesado", duplicado = true });

            var vinculacion = await _context.VinculacionesFlotaChat
                .FirstOrDefaultAsync(v => v.FlotaChatUsuarioId == evento.UsuarioId && v.TipoEntidad == "Conductor");

            var respuesta = new RespuestaFlotaChat
            {
                FlotaChatUsuarioId = evento.UsuarioId,
                ConductorId = vinculacion?.EntidadId,
                GrupoId = evento.GrupoId,
                Contenido = evento.Contenido ?? string.Empty,
                FechaRecibido = evento.Fecha,
                Procesado = false,
                IdEvento = idEvento,
                TipoMensaje = evento.TipoMensaje,
                MensajeId = mensajeId
            };
            _context.RespuestasFlotaChat.Add(respuesta);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return Ok(new { mensaje = "Evento ya procesado", duplicado = true });
            }

            var grupoPedidos = int.TryParse(_config["FlotaChat:GrupoPedidos"], out var g) ? g : 0;

            if (grupoPedidos > 0 && evento.GrupoId == grupoPedidos)
            {
                // Grupo Pedidos: confirmaciones. El servicio valida quién puede confirmar.
                await _pedidos.ProcesarAsync(evento);
                respuesta.Procesado = true;
            }
            else if (vinculacion != null && evento.TipoMensaje != "ubicacion" && !string.IsNullOrWhiteSpace(evento.Contenido))
            {
                // Cualquier otro lugar: el bot de conductores.
                await _bot.ProcesarAsync(vinculacion.EntidadId, evento.UsuarioId, evento.Contenido);
                respuesta.Procesado = true;
            }

            await _context.SaveChangesAsync();
            return Ok(new { mensaje = "Evento registrado", id = respuesta.Id });
        }

        // Firma = sha256 con el secreto sobre "timestamp.cuerpo". Rechaza eventos de más de 5 minutos.
        private bool FirmaValida(string secreto, string cuerpo)
        {
            var timestamp = Request.Headers["X-FlotaChat-Timestamp"].ToString();
            var firma = Request.Headers["X-FlotaChat-Firma"].ToString();
            if (!long.TryParse(timestamp, out var segundos) || !firma.StartsWith("sha256=")) return false;

            try
            {
                var hora = DateTimeOffset.FromUnixTimeSeconds(segundos);
                if ((DateTimeOffset.UtcNow - hora).Duration() > Tolerancia) return false;
            }
            catch (ArgumentOutOfRangeException)
            {
                return false;
            }

            var esperada = Convert.ToHexString(
                HMACSHA256.HashData(Encoding.UTF8.GetBytes(secreto), Encoding.UTF8.GetBytes($"{timestamp}.{cuerpo}")))
                .ToLowerInvariant();

            return CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(esperada), Encoding.UTF8.GetBytes(firma[7..]));
        }

        // Webhook viejo, sin firma: se retira en el paso 5.4.
        [HttpPost("recibir")]
        public async Task<IActionResult> Recibir([FromBody] WebhookRespuestaDto dto)
        {
            var esperado = _config["FlotaChat:WebhookSecreto"];
            var recibido = Request.Headers["X-Webhook-Secret"].ToString();
            if (string.IsNullOrEmpty(esperado) || string.IsNullOrEmpty(recibido) ||
                !CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(esperado), Encoding.UTF8.GetBytes(recibido)))
                return Unauthorized(new { mensaje = "Secreto de webhook inválido" });

            var vinculacion = await _context.VinculacionesFlotaChat
                .FirstOrDefaultAsync(v => v.FlotaChatUsuarioId == dto.UsuarioId && v.TipoEntidad == "Conductor");

            var respuesta = new RespuestaFlotaChat
            {
                FlotaChatUsuarioId = dto.UsuarioId,
                ConductorId = vinculacion?.EntidadId,
                GrupoId = dto.GrupoId,
                Contenido = dto.Contenido,
                FechaRecibido = dto.Fecha,
                Procesado = false
            };
            _context.RespuestasFlotaChat.Add(respuesta);
            await _context.SaveChangesAsync();

            if (vinculacion != null)
            {
                await _bot.ProcesarAsync(vinculacion.EntidadId, dto.UsuarioId, dto.Contenido);
                respuesta.Procesado = true;
                await _context.SaveChangesAsync();
            }

            return Ok(new { mensaje = "Respuesta registrada", id = respuesta.Id, conductorId = respuesta.ConductorId });
        }
    }
}