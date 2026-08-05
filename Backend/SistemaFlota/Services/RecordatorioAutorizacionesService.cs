using Microsoft.EntityFrameworkCore;

namespace SistemaFlota
{
    public class RecordatorioAutorizacionesService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IMensajeriaService _flotaChat;
        private readonly ILogger<RecordatorioAutorizacionesService> _logger;

        // Horas de chequeo (hora local Colombia)
        private static readonly TimeSpan HoraPausaManana = new(10, 0, 0);
        private static readonly TimeSpan HoraRecordatorioManana = new(11, 50, 0);
        private static readonly TimeSpan HoraVencimientoManana = new(12, 0, 0);
        private static readonly TimeSpan HoraPausaTarde = new(16, 0, 0);
        private static readonly TimeSpan HoraRecordatorioTarde = new(17, 50, 0);
        private static readonly TimeSpan HoraVencimientoTarde = new(18, 0, 0);
        private static readonly TimeSpan HoraResumenDiario = new(18, 30, 0);

        // Marca de la última acción ejecutada hoy, para no repetir en el mismo minuto
        private DateTime _ultimaFechaProcesada = DateTime.MinValue;
        private readonly HashSet<string> _accionesHoy = new();

        public RecordatorioAutorizacionesService(
            IServiceScopeFactory scopeFactory,
            IMensajeriaService flotaChat,
            ILogger<RecordatorioAutorizacionesService> logger)
        {
            _scopeFactory = scopeFactory;
            _flotaChat = flotaChat;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcesarCicloAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "? Error en RecordatorioAutorizacionesService");
                }

                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }

        private async Task ProcesarCicloAsync()
        {
            var ahora = DateTime.Now; // servidor ya está en TZ America/Bogota (ver Program.cs)
            var hoy = ahora.Date;

            // Reinicia el set de acciones cuando cambia el día
            if (hoy != _ultimaFechaProcesada)
            {
                _accionesHoy.Clear();
                _ultimaFechaProcesada = hoy;
            }

            var diaSemana = ahora.DayOfWeek;
            if (diaSemana == DayOfWeek.Sunday) return; // domingo no corre nada

            var horaActual = ahora.TimeOfDay;

            // -- Pausa activa (L-S, 10am y L-V 4pm) --------------------------
            await EjecutarSiToca("pausa_10", horaActual, HoraPausaManana, EnviarPausaActivaAsync);

            if (diaSemana != DayOfWeek.Saturday)
                await EjecutarSiToca("pausa_16", horaActual, HoraPausaTarde, EnviarPausaActivaAsync);

            // -- Recordatorio fin de ruta -------------------------------------
            await EjecutarSiToca("recordatorio_1150", horaActual, HoraRecordatorioManana, EnviarRecordatorioFinRutaAsync);

            if (diaSemana != DayOfWeek.Saturday)
                await EjecutarSiToca("recordatorio_1750", horaActual, HoraRecordatorioTarde, EnviarRecordatorioFinRutaAsync);

            // -- Vencimiento (L-V mediodía y 6pm; sábado NO vence) -----------
            if (diaSemana != DayOfWeek.Saturday)
            {
                await EjecutarSiToca("vencimiento_12", horaActual, HoraVencimientoManana, ProcesarVencimientoAsync);
                await EjecutarSiToca("vencimiento_18", horaActual, HoraVencimientoTarde, ProcesarVencimientoAsync);
                await EjecutarSiToca("resumen_diario", horaActual, HoraResumenDiario, EnviarResumenDiarioAsync);
            }

            // -- Sábado: seguimiento suave cada hora desde la 1pm hasta las 8pm -
            if (diaSemana == DayOfWeek.Saturday)
            {
                for (int h = 13; h <= 20; h++)
                {
                    var horaObjetivo = new TimeSpan(h, 0, 0);
                    await EjecutarSiToca($"seguimiento_sabado_{h}", horaActual, horaObjetivo, EnviarSeguimientoSabadoAsync);
                }
            }

            // -- Reintento y escalamiento (corre siempre, cada minuto) --------
            await RevisarEscalamientoAsync();
        }

        // Ejecuta la acción solo si la hora actual coincide (±1 min) con la hora objetivo
        // y no se ha ejecutado ya hoy (evita duplicados si el ciclo corre cada minuto).
        private async Task EjecutarSiToca(string clave, TimeSpan horaActual, TimeSpan horaObjetivo, Func<Task> accion)
        {
            if (_accionesHoy.Contains(clave)) return;
            if (horaActual < horaObjetivo || horaActual > horaObjetivo.Add(TimeSpan.FromMinutes(2))) return;

            _accionesHoy.Add(clave);
            _logger.LogInformation("? Ejecutando acción: {Clave}", clave);
            await accion();
        }

        // -- Pausa activa: a TODOS los conductores con estado Activo en el sistema -
        private async Task EnviarPausaActivaAsync()
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var conductores = await db.Conductores
                .Where(c => c.Estado == "Activo")
                .ToListAsync();

            foreach (var c in conductores)
            {
                if (string.IsNullOrWhiteSpace(c.Telefono))
                {
                    _logger.LogWarning("?? Conductor {Nombre} sin teléfono, no se pudo notificar (pausa activa)", c.Nombre);
                    continue;
                }
                var mensaje = $"?? Recordatorio de pausa activa\n\nHola {c.Nombre}, recuerda tomar unos minutos para estirarte y descansar la vista. ¡Cuídate en la vía!";
                await _flotaChat.EnviarMensajeAsync(c.Telefono, mensaje);
            }

            _logger.LogInformation("? Pausa activa enviada a {Cantidad} conductores", conductores.Count);
        }

        // -- Recordatorio de fin de ruta: autorizaciones activas --------------
        private async Task EnviarRecordatorioFinRutaAsync()
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var activas = await db.Autorizaciones
                .Include(a => a.Conductor)
                .Where(a => a.Estado == "Autorizado" && a.EstadoLlegada == null)
                .ToListAsync();

            foreach (var a in activas)
            {
                if (a.Conductor == null || string.IsNullOrWhiteSpace(a.Conductor.Telefono))
                {
                    _logger.LogWarning("?? Autorización #{Id} sin conductor/teléfono válido", a.Id);
                    continue;
                }
                var mensaje = $"?? RECORDATORIO AUTORIZACIÓN\n\nConductor: {a.Conductor.Nombre}\nDestino: {a.DestinoCompleto}\n\nPor favor confirma si ya terminaste tu ruta o sigues en camino.";
                await _flotaChat.EnviarMensajeAsync(a.Conductor.Telefono, mensaje);

                a.FechaUltimoRecordatorio = DateTime.Now;
                a.IntentosRecordatorio = 1;
                a.Escalado = false;
            }

            await db.SaveChangesAsync();
            _logger.LogInformation("? Recordatorio fin de ruta enviado a {Cantidad} autorizaciones activas", activas.Count);
        }

        // -- Vencimiento: marca y alerta (L-V únicamente) ---------------------
        private async Task ProcesarVencimientoAsync()
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var activas = await db.Autorizaciones
                .Include(a => a.Conductor)
                .Where(a => a.Estado == "Autorizado" && a.EstadoLlegada == null)
                .ToListAsync();

            foreach (var a in activas)
            {
                a.EstadoLlegada = "Vencido"; // ajustar si prefieres otro valor de estado

                if (a.Conductor != null && !string.IsNullOrWhiteSpace(a.Conductor.Telefono))
                {
                    var mensaje = $"?? AUTORIZACIÓN VENCIDA\n\nConductor: {a.Conductor.Nombre}\nDestino: {a.DestinoCompleto}\n\nPor favor confirma tu estado o comunícate con logística de inmediato.";
                    await _flotaChat.EnviarMensajeAsync(a.Conductor.Telefono, mensaje);
                }
            }

            await db.SaveChangesAsync();
            _logger.LogInformation("?? {Cantidad} autorizaciones marcadas como vencidas", activas.Count);
        }
        // -- Resumen diario: estadísticas del día a los Contactos de Notificación --
        private async Task EnviarResumenDiarioAsync()
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var hoy = DateTime.Now.Date;
            var autorizacionesHoy = await db.Autorizaciones
                .Where(a => a.FechaCreacion.Date == hoy)
                .ToListAsync();

            var total = autorizacionesHoy.Count;
            var completadas = autorizacionesHoy.Count(a => a.EstadoLlegada == "ReportadaLlegada" || a.EstadoLlegada == "Completada");
            var vencidas = autorizacionesHoy.Count(a => a.EstadoLlegada == "Vencido");
            var conNovedad = autorizacionesHoy.Count(a => !string.IsNullOrWhiteSpace(a.NovedadesViaje));
            var enCurso = autorizacionesHoy.Count(a => a.Estado == "Autorizado" && a.EstadoLlegada == null);

            var mensaje = $"?? RESUMEN DEL DÍA — {hoy:dd/MM/yyyy}\n\n" +
                          $"Total autorizaciones: {total}\n" +
                          $"? Completadas: {completadas}\n" +
                          $"?? Vencidas: {vencidas}\n" +
                          $"?? Con novedad: {conNovedad}\n" +
                          $"?? Aún en curso: {enCurso}";

            var contactos = await db.ContactosNotificacion
                .Where(c => c.Activo && c.RecibeIncidentes)
                .Select(c => c.NumeroWhatsApp)
                .ToListAsync();

            if (contactos.Any())
                await _flotaChat.EnviarAMultiplesAsync(contactos, mensaje);

            _logger.LogInformation("?? Resumen diario enviado — Total: {Total}, Completadas: {Completadas}, Vencidas: {Vencidas}", total, completadas, vencidas);
        }
        // -- Sábado: seguimiento suave, sin marcar vencido --------------------
        private async Task EnviarSeguimientoSabadoAsync()
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var activas = await db.Autorizaciones
                .Include(a => a.Conductor)
                .Where(a => a.Estado == "Autorizado" && a.EstadoLlegada == null)
                .ToListAsync();

            foreach (var a in activas)
            {
                if (a.Conductor == null || string.IsNullOrWhiteSpace(a.Conductor.Telefono)) continue;
                var mensaje = $"?? Hola {a.Conductor.Nombre}, ¿ya llegaste o terminaste tu ruta? Cuéntanos para actualizar tu estado.";
                await _flotaChat.EnviarMensajeAsync(a.Conductor.Telefono, mensaje);
            }

            _logger.LogInformation("? Seguimiento suave sábado enviado a {Cantidad} autorizaciones", activas.Count);
        }

        // -- Si no responde en 15 min: reintenta una vez; si sigue sin responder, escala --
        private async Task RevisarEscalamientoAsync()
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var candidatas = await db.Autorizaciones
                .Include(a => a.Conductor)
                .Where(a => a.Estado == "Autorizado"
                         && a.EstadoLlegada == null
                         && a.FechaUltimoRecordatorio != null
                         && !a.Escalado)
                .ToListAsync();

            foreach (var a in candidatas)
            {
                var minutosSinResponder = (DateTime.Now - a.FechaUltimoRecordatorio!.Value).TotalMinutes;
                if (minutosSinResponder < 15) continue;

                if (a.IntentosRecordatorio == 1)
                {
                    // Reintento
                    if (a.Conductor != null && !string.IsNullOrWhiteSpace(a.Conductor.Telefono))
                    {
                        var mensaje = $"?? RECORDATORIO (reintento)\n\nConductor: {a.Conductor.Nombre}\nDestino: {a.DestinoCompleto}\n\nNo hemos recibido tu respuesta. Por favor confirma tu estado.";
                        await _flotaChat.EnviarMensajeAsync(a.Conductor.Telefono, mensaje);
                    }
                    a.FechaUltimoRecordatorio = DateTime.Now;
                    a.IntentosRecordatorio = 2;
                    _logger.LogInformation("?? Reintento enviado — Autorización #{Id}", a.Id);
                }
                else
                {
                    // Escala a supervisores
                    var contactos = await db.ContactosNotificacion
                        .Where(c => c.Activo && c.RecibeIncidentes)
                        .Select(c => c.NumeroWhatsApp)
                        .ToListAsync();

                    if (contactos.Any())
                    {
                        var mensajeEscalado = $"?? CONDUCTOR SIN RESPUESTA\n\nConductor: {a.Conductor?.Nombre ?? "-"}\nDestino: {a.DestinoCompleto}\nAutorización #{a.Id}\n\nNo ha respondido en más de 30 minutos. Por favor verificar.";
                        await _flotaChat.EnviarAMultiplesAsync(contactos, mensajeEscalado);
                    }

                    a.Escalado = true;
                    _logger.LogWarning("?? Autorización #{Id} ESCALADA — sin respuesta del conductor", a.Id);
                }
            }

            await db.SaveChangesAsync();
        }
    }
}
