using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace SistemaFlota.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [AllowAnonymous]
    public class WebhookFlotaChatController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _config;

        public WebhookFlotaChatController(AppDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        private bool SecretoValido()
        {
            var secretoEsperado = _config["FlotaChat:WebhookSecreto"];
            var secretoRecibido = Request.Headers["X-Webhook-Secret"].ToString();
            if (string.IsNullOrEmpty(secretoEsperado)) return true;
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
                await ProcesarRespuestaAsync(vinculacion.EntidadId, dto.Contenido);
                respuesta.Procesado = true;
                await _context.SaveChangesAsync();
            }

            return Ok(new { mensaje = "Respuesta registrada", id = respuesta.Id, conductorId = respuesta.ConductorId });
        }

        // ── Interpreta el botón/mensaje del conductor y actualiza la Autorización ──
        private async Task ProcesarRespuestaAsync(int conductorId, string contenido)
        {
            var texto = contenido.Trim().ToLower();

            var autorizacionActiva = await _context.Autorizaciones
                .Where(a => a.ConductorId == conductorId && a.Estado == "Autorizado" && a.EstadoLlegada == null)
                .OrderByDescending(a => a.FechaCreacion)
                .FirstOrDefaultAsync();

            if (autorizacionActiva == null) return; // no tiene autorización activa, nada que hacer

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
            // "Entrega completada" y "En punto de entrega" quedan solo como
            // registro en RespuestasFlotaChat, sin cambiar la Autorización.

            await _context.SaveChangesAsync();
        }
    }

    public record WebhookRespuestaDto(
        int GrupoId,
        int UsuarioId,
        string NombreConductor,
        string Contenido,
        DateTime Fecha
    );
}