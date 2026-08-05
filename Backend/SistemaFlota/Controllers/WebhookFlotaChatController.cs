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
        private readonly IMensajeriaService _mensajeria;

        private static readonly string[] PalabrasClaveSalida = { "salir", "salgo", "ruta", "autorizacion", "autorización" };

        public WebhookFlotaChatController(AppDbContext context, IConfiguration config, IMensajeriaService mensajeria)
        {
            _context = context;
            _config = config;
            _mensajeria = mensajeria;
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
                .Include(a => a.Conductor)
                .Include(a => a.Vehiculo)
                .Where(a => a.ConductorId == conductorId && a.Estado == "Autorizado" && a.EstadoLlegada == null)
                .OrderByDescending(a => a.FechaCreacion)
                .FirstOrDefaultAsync();

            // ── Si tiene autorización activa, maneja las respuestas normales ──
            if (autorizacionActiva != null)
            {
                if (texto.Contains("saliendo de bodega"))
                {
                    autorizacionActiva.FechaSalidaReal = DateTime.Now;
                    await ResponderAsync(flotaChatUsuarioId, "🚛 Registrado: saliendo de bodega. ¡Buen viaje!");
                }
                else if (texto.Contains("ruta completada") || texto.Contains("entrega completada"))
                {
                    autorizacionActiva.EstadoLlegada = "ReportadaLlegada";
                    autorizacionActiva.FechaReporteLlegada = DateTime.Now;
                    await ResponderAsync(flotaChatUsuarioId,
                        $"✅ Ruta terminada\nDestino: {autorizacionActiva.DestinoCompleto ?? "-"}\n\n¡Gracias por confirmar!");
                }
                else if (texto.Contains("en punto de entrega"))
                {
                    await ResponderAsync(flotaChatUsuarioId, "📍 Registrado: en punto de entrega.");
                }
                else if (texto.Contains("necesito asistencia") ||
                         texto.Contains("requiero combustible") ||
                         texto.Contains("falla mecánica") ||
                         texto.Contains("falla mecanica"))
                {
                    autorizacionActiva.NovedadesViaje = (autorizacionActiva.NovedadesViaje ?? "") +
                        $"\n[{DateTime.Now:dd/MM HH:mm}] {contenido}";
                    await ResponderAsync(flotaChatUsuarioId, "⚠️ Novedad registrada. Alguien te contactará pronto.");
                }
                else if (texto.Contains("llegaré tarde") || texto.Contains("llegare tarde"))
                {
                    autorizacionActiva.NovedadesViaje = (autorizacionActiva.NovedadesViaje ?? "") +
                        $"\n[{DateTime.Now:dd/MM HH:mm}] Aviso: llegará tarde";
                    await ResponderAsync(flotaChatUsuarioId, "🕐 Aviso de retraso registrado.");
                }
                else
                {
                    var vehiculoTxt = autorizacionActiva.Vehiculo?.Placa ?? "-";
                    var destinoTxt = autorizacionActiva.DestinoCompleto ?? "-";
                    await ResponderAsync(flotaChatUsuarioId,
                        $"📋 Ya tienes una autorización pendiente:\n" +
                        $"🚗 Vehículo: {vehiculoTxt}\n📍 Destino: {destinoTxt}\n\n" +
                        "Puedes escribir:\n" +
                        "🚛 Saliendo de bodega\n" +
                        "✅ Entrega completada\n" +
                        "📍 En punto de entrega\n" +
                        "⚠️ Necesito asistencia\n" +
                        "⛽ Requiero combustible\n" +
                        "🔧 Vehículo con falla mecánica\n" +
                        "🕐 Llegaré tarde\n" +
                        "✔️ Ruta completada");
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
            // ── Paso 1: Confirmación ──
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

            // ── Paso 2: Placa ──
            if (conversacion.Paso == "EsperandoPlaca")
            {
                var placa = texto.Trim().ToUpper().Replace(" ", "");
                var vehiculo = await _context.Vehiculos.FirstOrDefaultAsync(v => v.Placa.ToUpper().Replace(" ", "") == placa);

                if (vehiculo == null)
                {
                    await ResponderAsync(flotaChatUsuarioId, $"❌ No encontré el vehículo con placa {placa}. Intenta de nuevo:");
                    return;
                }

                conversacion.VehiculoIdTemp = vehiculo.Id;
                conversacion.Paso = "EsperandoDestino";
                conversacion.FechaExpiracion = DateTime.Now.AddMinutes(10);
                await _context.SaveChangesAsync();
                await ResponderAsync(flotaChatUsuarioId, "📍 Indica el destino del viaje:");
                return;
            }

            // ── Paso 3: Destino ──
            if (conversacion.Paso == "EsperandoDestino")
            {
                if (string.IsNullOrWhiteSpace(texto))
                {
                    await ResponderAsync(flotaChatUsuarioId, "❌ El destino no puede estar vacío. Indica el destino del viaje:");
                    return;
                }

                conversacion.DestinoTemp = texto.Trim();
                conversacion.Paso = "EsperandoTipoVuelta";
                conversacion.FechaExpiracion = DateTime.Now.AddMinutes(10);
                await _context.SaveChangesAsync();
                await ResponderAsync(flotaChatUsuarioId,
                    "📋 ¿Cuál es el tipo de vuelta?\n\n" +
                    "• Solo Entrega\n" +
                    "• Mixta\n" +
                    "• Recoge y Entrega\n" +
                    "• Mensajería\n" +
                    "• Solo Recoge");
                return;
            }

            // ── Paso 4: Tipo de Vuelta ──
            if (conversacion.Paso == "EsperandoTipoVuelta")
            {
                string? tipoVuelta = null;

                if (texto.Contains("solo entrega")) tipoVuelta = "Solo Entrega";
                else if (texto.Contains("mixta")) tipoVuelta = "Mixta";
                else if (texto.Contains("recoge") && texto.Contains("entrega")) tipoVuelta = "Recoge y Entrega";
                else if (texto.Contains("mensajer")) tipoVuelta = "Mensajería";
                else if (texto.Contains("solo recoge") || texto.Contains("solo recoje")) tipoVuelta = "Solo Recoge";

                if (tipoVuelta == null)
                {
                    await ResponderAsync(flotaChatUsuarioId,
                        "❌ No reconocí el tipo de vuelta. Escribe una de estas opciones:\n\n" +
                        "• Solo Entrega\n• Mixta\n• Recoge y Entrega\n• Mensajería\n• Solo Recoge");
                    return;
                }

                var conductor = await _context.Conductores.FindAsync(conductorId);
                var vehiculoFinal = await _context.Vehiculos.FindAsync(conversacion.VehiculoIdTemp!.Value);
                var ahora = DateTime.Now;

                var nueva = new Autorizacion
                {
                    ConductorId = conductorId,
                    VehiculoId = conversacion.VehiculoIdTemp.Value,
                    TipoVuelta = tipoVuelta,
                    DestinoCompleto = conversacion.DestinoTemp,
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
                    $"✅ Autorización creada para {conductor?.Nombre ?? "-"}\n" +
                    $"🚗 Vehículo: {vehiculoFinal?.Placa ?? "-"}\n" +
                    $"📍 Destino: {conversacion.DestinoTemp}\n" +
                    $"📋 Tipo: {tipoVuelta}\n\n" +
                    "🚛 ¡Buen viaje!");
            }
        }

        private async Task ResponderAsync(int flotaChatUsuarioId, string mensaje)
        {
            var vinculacion = await _context.VinculacionesFlotaChat
                .FirstOrDefaultAsync(v => v.FlotaChatUsuarioId == flotaChatUsuarioId);
            if (vinculacion?.Telefono != null)
                await _mensajeria.EnviarMensajeAsync(vinculacion.Telefono, mensaje);
        }
    }
}