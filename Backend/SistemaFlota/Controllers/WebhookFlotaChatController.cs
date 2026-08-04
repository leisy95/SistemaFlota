using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using SistemaFlota.Models;
using SistemaFlota.DTOs;

namespace SistemaFlota.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [AllowAnonymous]
    public class WebhookFlotaChatController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _config;
        private readonly ITwilioService _twilio;

        private static readonly string[] PalabrasClaveSalida = { "salir", "salgo", "ruta", "autorizacion", "autorización" };

        public WebhookFlotaChatController(AppDbContext context, IConfiguration config, ITwilioService twilio)
        {
            _context = context;
            _config = config;
            _twilio = twilio;
        }

        private bool SecretoValido()
        {
            var secretoEsperado = _config["FlotaChat:WebhookSecreto"];
            if (string.IsNullOrEmpty(secretoEsperado))
                return false;
            var secretoRecibido = Request.Headers["X-Webhook-Secret"].ToString();
            return !string.IsNullOrEmpty(secretoRecibido) && secretoRecibido == secretoEsperado;
        }

        [HttpPost("recibir")]
        public async Task<IActionResult> Recibir([FromBody] WebhookRespuestaDto dto)
        {
            if (!SecretoValido())
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
                await ProcesarRespuestaAsync(vinculacion.EntidadId, dto.UsuarioId, dto.Contenido);
                respuesta.Procesado = true;
                await _context.SaveChangesAsync();
            }

            return Ok(new { mensaje = "Respuesta registrada", id = respuesta.Id, conductorId = respuesta.ConductorId });
        }

        // ── Interpreta el botón/mensaje del conductor ──────────────────────────
        private async Task ProcesarRespuestaAsync(int conductorId, int flotaChatUsuarioId, string contenido)
        {
            var texto = contenido.Trim().ToLower();

            var autorizacionActiva = await _context.Autorizaciones
                .Where(a => a.ConductorId == conductorId && a.Estado == "Autorizado" && a.EstadoLlegada == null)
                .OrderByDescending(a => a.FechaCreacion)
                .FirstOrDefaultAsync();

            // ── Si tiene autorización activa, maneja las respuestas normales ──
            if (autorizacionActiva != null)
            {
                if (texto.Contains("saliendo de bodega"))
                {
                    autorizacionActiva.FechaSalidaReal = DateTime.Now;
                }
                else if (texto.Contains("ruta completada"))
                {
                    autorizacionActiva.EstadoLlegada = "ReportadaLlegada";
                    autorizacionActiva.FechaReporteLlegada = DateTime.Now;
                }
                else if (texto.Contains("necesito asistencia") ||
                         texto.Contains("requiero combustible") ||
                         texto.Contains("falla mecánica") ||
                         texto.Contains("falla mecanica"))
                {
                    autorizacionActiva.NovedadesViaje = (autorizacionActiva.NovedadesViaje ?? "") +
                        $"\n[{DateTime.Now:dd/MM HH:mm}] {contenido}";
                }
                else if (texto.Contains("llegaré tarde") || texto.Contains("llegare tarde"))
                {
                    autorizacionActiva.NovedadesViaje = (autorizacionActiva.NovedadesViaje ?? "") +
                        $"\n[{DateTime.Now:dd/MM HH:mm}] Aviso: llegará tarde";
                }
                await _context.SaveChangesAsync();
                return;
            }

            // ── Sin autorización activa: revisa si hay una conversación en curso ──
            var conversacion = (await _context.ConversacionesFlotaChat
                .Where(c => c.FlotaChatUsuarioId == flotaChatUsuarioId)
                .OrderByDescending(c => c.FechaInicio)
                .ToListAsync())
                .FirstOrDefault(c => c.FechaExpiracion > DateTime.Now);
            if (conversacion != null)
            {

                await ContinuarConversacionAsync(conversacion, conductorId, flotaChatUsuarioId, texto);
                return;
            }

            // ── No hay conversación activa: revisa si el mensaje pide iniciar ruta ──
            if (!PalabrasClaveSalida.Any(p => texto.Contains(p))) return;

            // ── Validar que no tenga OTRA autorización pendiente sin resolver ──
            var pendiente = await _context.Autorizaciones
                .Where(a => a.ConductorId == conductorId &&
                    (a.Estado == "Pendiente" || a.Estado == "Bodega" || a.Estado == "Porteria"))
                .FirstOrDefaultAsync();

            if (pendiente != null)
            {
                await ResponderAsync(flotaChatUsuarioId,
                    "⚠️ Ya tienes una autorización en proceso, esperando ser aprobada. Por favor espera a que se complete.");
                return;
            }

            // ── Todo libre: inicia la conversación de nueva autorización ──
            _context.ConversacionesFlotaChat.Add(new ConversacionFlotaChat
            {
                FlotaChatUsuarioId = flotaChatUsuarioId,
                Paso = "EsperandoConfirmacion",
                FechaInicio = DateTime.Now,
                FechaExpiracion = DateTime.Now.AddMinutes(10)
            });
            await _context.SaveChangesAsync();

            await ResponderAsync(flotaChatUsuarioId,
                "🚚 ¿Deseas iniciar una nueva autorización de salida?\n\nResponde *SI* para continuar.");
        }

        // ── Continúa una conversación de creación de autorización en curso ─────
        private async Task ContinuarConversacionAsync(ConversacionFlotaChat conversacion, int conductorId, int flotaChatUsuarioId, string texto)
        {
            if (conversacion.Paso == "EsperandoConfirmacion")
            {
                if (texto.Trim() == "si" || texto.Trim() == "sí")
                {
                    conversacion.Paso = "EsperandoPlaca";
                    conversacion.FechaExpiracion = DateTime.Now.AddMinutes(10);
                    await _context.SaveChangesAsync();
                    await ResponderAsync(flotaChatUsuarioId, "🚗 Indica la placa del vehículo (ej: ABC123):");
                }
                else
                {
                    _context.ConversacionesFlotaChat.Remove(conversacion);
                    await _context.SaveChangesAsync();
                    await ResponderAsync(flotaChatUsuarioId, "Entendido, no se creó ninguna autorización.");
                }
                return;
            }

            if (conversacion.Paso == "EsperandoPlaca")
            {
                var placa = texto.Trim().ToUpper().Replace(" ", "");
                var vehiculo = await _context.Vehiculos.FirstOrDefaultAsync(v => v.Placa.ToUpper().Replace(" ", "") == placa);

                if (vehiculo == null)
                {
                    await ResponderAsync(flotaChatUsuarioId, $"❌ No encontré el vehículo con placa {placa}. Intenta de nuevo:");
                    return;
                }

                var conductor = await _context.Conductores.FindAsync(conductorId);
                var ahora = DateTime.Now;

                var nueva = new Autorizacion
                {
                    ConductorId = conductorId,
                    VehiculoId = vehiculo.Id,
                    TipoVuelta = "Mensajería",
                    DestinoCompleto = "Generada desde chat",
                    DescripcionCarga = string.Empty,
                    Estado = "Autorizado",
                    FechaCreacion = ahora,
                    FechaSalidaReal = ahora,
                    UsuarioPorteria = "Chat FlotaChat",
                    FechaPorteria = ahora
                };
                _context.Autorizaciones.Add(nueva);

                _context.ConversacionesFlotaChat.Remove(conversacion);
                await _context.SaveChangesAsync();

                await ResponderAsync(flotaChatUsuarioId,
                    $"✅ Autorización creada para {conductor?.Nombre ?? "-"} con el vehículo {vehiculo.Placa}.\n\n¡Buen viaje!");
            }
        }

        private async Task ResponderAsync(int flotaChatUsuarioId, string mensaje)
        {
            var vinculacion = await _context.VinculacionesFlotaChat
                .FirstOrDefaultAsync(v => v.FlotaChatUsuarioId == flotaChatUsuarioId);
            if (vinculacion?.Telefono != null)
                await _twilio.EnviarMensajeAsync(vinculacion.Telefono, mensaje);
        }
    }
}