using System.Globalization;
using Microsoft.EntityFrameworkCore;
using SistemaFlota.Models;

namespace SistemaFlota
{
    
    public class BotConductorService
    {
        private static readonly TimeSpan Duracion = TimeSpan.FromMinutes(10);
        private static readonly string[] TiposVuelta = { "Solo Entrega", "Mixta", "Recoge y Entrega", "Mensajería", "Solo Recoge" };
        private const string ListaTipos = "1️⃣ Solo Entrega\n2️⃣ Mixta\n3️⃣ Recoge y Entrega\n4️⃣ Mensajería\n5️⃣ Solo Recoge";
        private const string OpcionesViaje =
            "Puedes escribir:\n" +
            "🚛 Saliendo de bodega\n" +
            "📍 En punto de entrega\n" +
            "✅ Ruta completada\n" +
            "⚠️ Necesito asistencia\n" +
            "⛽ Requiero combustible\n" +
            "🔧 Vehículo con falla mecánica\n" +
            "🕐 Llegaré tarde";

        private readonly AppDbContext _context;
        private readonly IMensajeriaService _mensajeria;

        public BotConductorService(AppDbContext context, IMensajeriaService mensajeria)
        {
            _context = context;
            _mensajeria = mensajeria;
        }

        public async Task ProcesarAsync(int conductorId, int flotaChatUsuarioId, string contenido)
        {
            var t = InterpreteConductor.Normalizar(contenido);
            if (t == "") return;

            var activa = await _context.Autorizaciones
                .Include(a => a.Vehiculo)
                .Where(a => a.ConductorId == conductorId && a.Estado == "Autorizado" && a.EstadoLlegada == null)
                .OrderByDescending(a => a.FechaCreacion)
                .FirstOrDefaultAsync();

            if (activa != null)
            {
                await ManejarViajeActivoAsync(activa, flotaChatUsuarioId, contenido, t);
                return;
            }

            var conversacion = (await _context.ConversacionesFlotaChat
                .Where(c => c.FlotaChatUsuarioId == flotaChatUsuarioId)
                .OrderByDescending(c => c.FechaInicio)
                .ToListAsync())
                .FirstOrDefault(c => c.FechaExpiracion > DateTime.Now);

            if (conversacion != null)
            {
                await ContinuarConversacionAsync(conversacion, conductorId, flotaChatUsuarioId, contenido, t);
                return;
            }

            if (InterpreteConductor.EsAyuda(t))
            {
                await ResponderAsync(flotaChatUsuarioId,
                    "🚚 Escribe *salgo* para iniciar una autorización de salida.\n\n" +
                    "También puedes decirlo completo, por ejemplo:\n*salgo ruta solo entrega Pereira ABC123*");
                return;
            }

            if (!InterpreteConductor.PideIniciarSalida(t)) return;

            await IniciarSalidaAsync(conductorId, flotaChatUsuarioId, t);
        }

        // ── Con autorización activa ─────────────────────────────────────────

        private async Task ManejarViajeActivoAsync(Autorizacion activa, int usuarioId, string contenido, string t)
        {
            switch (InterpreteConductor.DetectarDuranteViaje(t))
            {
                case IntencionViaje.Salida:
                    activa.FechaSalidaReal = DateTime.Now;
                    await ResponderAsync(usuarioId, "🚛 Registrado: saliendo de bodega. ¡Buen viaje!");
                    break;

                case IntencionViaje.PuntoEntrega:
                    await ResponderAsync(usuarioId, "📍 Registrado: en punto de entrega.");
                    break;

                case IntencionViaje.Llegada:
                    activa.EstadoLlegada = "ReportadaLlegada";
                    activa.FechaReporteLlegada = DateTime.Now;
                    await ResponderAsync(usuarioId,
                        $"✅ Ruta terminada\nDestino: {(string.IsNullOrWhiteSpace(activa.DestinoCompleto) ? "-" : activa.DestinoCompleto)}\n\n¡Gracias por confirmar!");
                    break;

                case IntencionViaje.Novedad:
                    activa.NovedadesViaje = (activa.NovedadesViaje ?? "") + $"\n[{DateTime.Now:dd/MM HH:mm}] {contenido}";
                    await ResponderAsync(usuarioId, "⚠️ Novedad registrada. Alguien te contactará pronto.");
                    break;

                case IntencionViaje.Retraso:
                    activa.NovedadesViaje = (activa.NovedadesViaje ?? "") + $"\n[{DateTime.Now:dd/MM HH:mm}] Aviso: llegará tarde";
                    await ResponderAsync(usuarioId, "🕐 Aviso de retraso registrado.");
                    break;

                default:
                    await ResponderAsync(usuarioId,
                        $"📋 Tienes un viaje activo:\n" +
                        $"🚗 Vehículo: {activa.Vehiculo?.Placa ?? "-"}\n" +
                        $"📍 Destino: {(string.IsNullOrWhiteSpace(activa.DestinoCompleto) ? "-" : activa.DestinoCompleto)}\n\n" +
                        OpcionesViaje);
                    break;
            }
            await _context.SaveChangesAsync();
        }

        // ── Pedir una salida ────────────────────────────────────────────────

        private async Task IniciarSalidaAsync(int conductorId, int usuarioId, string t)
        {
            var haceVeinteMinutos = DateTime.Now.AddMinutes(-20);

            var pendienteViaje = await _context.Autorizaciones
                .AnyAsync(a => a.ConductorId == conductorId && !a.AplazadaPorChat &&
                    a.Estado == "Autorizado" && a.EstadoLlegada == "ReportadaLlegada" &&
                    a.FechaReporteLlegada != null && a.FechaReporteLlegada > haceVeinteMinutos);

            if (pendienteViaje)
            {
                await ResponderAsync(usuarioId,
                    "⚠️ Ya tienes un viaje sin cerrar (esperando confirmación de portería). Por favor espera unos minutos a que se confirme tu llegada.");
                return;
            }

            var conversacion = new ConversacionFlotaChat
            {
                FlotaChatUsuarioId = usuarioId,
                FechaInicio = DateTime.Now,
                FechaExpiracion = DateTime.Now.Add(Duracion)
            };
            var aviso = await PrellenarAsync(conversacion, t);

            var pendienteTramite = await _context.Autorizaciones
                .Where(a => a.ConductorId == conductorId && !a.AplazadaPorChat &&
                    (a.Estado == "Pendiente" || a.Estado == "Bodega" || a.Estado == "Porteria"))
                .FirstOrDefaultAsync();

            if (pendienteTramite != null)
            {
                conversacion.Paso = "EsperandoDecisionPendiente";
                conversacion.AutorizacionPendienteIdTemp = pendienteTramite.Id;
                _context.ConversacionesFlotaChat.Add(conversacion);
                await _context.SaveChangesAsync();

                await ResponderAsync(usuarioId,
                    $"📋 Ya tienes una autorización pendiente:\n" +
                    $"📍 Destino: {pendienteTramite.DestinoCompleto}\n" +
                    $"📋 Tipo: {pendienteTramite.TipoVuelta}\n" +
                    $"⏳ Esperando: {pendienteTramite.Estado}\n\n" +
                    "¿Qué deseas hacer?\n" +
                    "1️⃣ Seguir esperando esa autorización\n" +
                    "2️⃣ Aplazarla y crear una nueva (me asignaron otra vuelta)");
                return;
            }

            conversacion.Paso = "EsperandoConfirmacion";
            _context.ConversacionesFlotaChat.Add(conversacion);
            await _context.SaveChangesAsync();

            await ResponderAsync(usuarioId, (aviso != null ? aviso + "\n\n" : "") + await PreguntaConfirmacionAsync(conversacion));
        }

        
        private async Task<string?> PrellenarAsync(ConversacionFlotaChat c, string t)
        {
            string? aviso = null;

            var placa = InterpreteConductor.DetectarPlaca(t);
            if (placa != null)
            {
                var (vehiculo, error) = await BuscarVehiculoLibreAsync(placa);
                if (vehiculo != null) c.VehiculoIdTemp = vehiculo.Id;
                else aviso = error;
            }

            var destino = InterpreteConductor.DetectarDestino(t);
            if (destino != null) c.DestinoTemp = destino;

            var tipo = InterpreteConductor.DetectarTipoVuelta(t);
            if (tipo != null) c.TipoVueltaTemp = tipo;

            return aviso;
        }

        private async Task<string> PreguntaConfirmacionAsync(ConversacionFlotaChat c)
        {
            var lineas = new List<string>();
            if (c.VehiculoIdTemp != null)
            {
                var vehiculo = await _context.Vehiculos.FindAsync(c.VehiculoIdTemp.Value);
                if (vehiculo != null) lineas.Add($"🚗 Vehículo: {vehiculo.Placa}");
            }
            if (!string.IsNullOrWhiteSpace(c.DestinoTemp)) lineas.Add($"📍 Destino: {c.DestinoTemp}");
            if (c.TipoVueltaTemp != null) lineas.Add($"📋 Tipo: {c.TipoVueltaTemp}");

            var entendi = lineas.Count > 0 ? "Entendí:\n" + string.Join("\n", lineas) + "\n\n" : "";
            return $"🚚 ¿Deseas iniciar una nueva autorización de salida?\n\n{entendi}Responde *SI* para continuar.";
        }

        // ── Conversación en curso ───────────────────────────────────────────

        private async Task ContinuarConversacionAsync(ConversacionFlotaChat c, int conductorId, int usuarioId, string contenido, string t)
        {
            if (InterpreteConductor.EsCancelar(t))
            {
                _context.ConversacionesFlotaChat.Remove(c);
                await _context.SaveChangesAsync();
                await ResponderAsync(usuarioId, "❌ Proceso cancelado. Puedes escribir *salgo* cuando quieras iniciar de nuevo.");
                return;
            }

            switch (c.Paso)
            {
                case "EsperandoDecisionPendiente":
                    await DecidirPendienteAsync(c, usuarioId, t);
                    return;

                case "EsperandoConfirmacion":
                    if (InterpreteConductor.EsAfirmativo(t))
                    {
                        await PrellenarAsync(c, t);   // "si, para Manizales" también sirve
                        await AvanzarAsync(c, conductorId, usuarioId);
                    }
                    else if (InterpreteConductor.EsNegativo(t))
                    {
                        var teniaDatos = c.VehiculoIdTemp != null || c.DestinoTemp != null || c.TipoVueltaTemp != null;
                        if (teniaDatos)
                        {
                            c.VehiculoIdTemp = null;
                            c.DestinoTemp = null;
                            c.TipoVueltaTemp = null;
                            await ResponderAsync(usuarioId, "De acuerdo, vamos paso a paso.");
                            await AvanzarAsync(c, conductorId, usuarioId);
                        }
                        else
                        {
                            _context.ConversacionesFlotaChat.Remove(c);
                            await _context.SaveChangesAsync();
                            await ResponderAsync(usuarioId, "Entendido, no se creó ninguna autorización.");
                        }
                    }
                    else
                    {
                        await ResponderAsync(usuarioId, "Responde *SI* para continuar o *cancelar* para salir.");
                    }
                    return;

                case "EsperandoPlaca":
                    var placa = InterpreteConductor.DetectarPlaca(t) ?? t.Replace(" ", "").ToUpperInvariant();
                    var (vehiculo, error) = await BuscarVehiculoLibreAsync(placa);
                    if (vehiculo == null)
                    {
                        await ResponderAsync(usuarioId, error!);
                        return;
                    }
                    c.VehiculoIdTemp = vehiculo.Id;
                    await AvanzarAsync(c, conductorId, usuarioId);
                    return;

                case "EsperandoDestino":
                    var destino = contenido.Trim();
                    if (destino.Length == 0)
                    {
                        await ResponderAsync(usuarioId, "❌ El destino no puede estar vacío. Indica el destino del viaje:");
                        return;
                    }
                    c.DestinoTemp = destino.Length > 200 ? destino[..200] : destino;
                    await AvanzarAsync(c, conductorId, usuarioId);
                    return;

                case "EsperandoTipoVuelta":
                    var tipo = int.TryParse(t, out var n) && n >= 1 && n <= TiposVuelta.Length
                        ? TiposVuelta[n - 1]
                        : InterpreteConductor.DetectarTipoVuelta(t);
                    if (tipo == null)
                    {
                        await ResponderAsync(usuarioId, "❌ No reconocí el tipo de vuelta. Escribe el número o el nombre:\n\n" + ListaTipos);
                        return;
                    }
                    c.TipoVueltaTemp = tipo;
                    await AvanzarAsync(c, conductorId, usuarioId);
                    return;
            }
        }

        private async Task DecidirPendienteAsync(ConversacionFlotaChat c, int usuarioId, string t)
        {
            var seguir = t == "1" || InterpreteConductor.Normalizar(t) is "uno" || t.Contains("seguir") || t.Contains("esperar") || t.Contains("sigo");
            var aplazar = t == "2" || t == "dos" || t.Contains("aplaz") || t.Contains("nueva") || t.Contains("otra");

            if (seguir && !aplazar)
            {
                _context.ConversacionesFlotaChat.Remove(c);
                await _context.SaveChangesAsync();
                await ResponderAsync(usuarioId, "👍 Entendido, sigue esperando la aprobación de tu autorización pendiente.");
                return;
            }

            if (aplazar)
            {
                if (c.AutorizacionPendienteIdTemp != null)
                {
                    var anterior = await _context.Autorizaciones.FindAsync(c.AutorizacionPendienteIdTemp.Value);
                    if (anterior != null) anterior.AplazadaPorChat = true;
                }

                c.Paso = "EsperandoConfirmacion";
                c.AutorizacionPendienteIdTemp = null;
                c.FechaExpiracion = DateTime.Now.Add(Duracion);
                await _context.SaveChangesAsync();

                await ResponderAsync(usuarioId, "✅ Autorización anterior aplazada.\n\n" + await PreguntaConfirmacionAsync(c));
                return;
            }

            await ResponderAsync(usuarioId, "❌ No entendí tu respuesta. Escribe *1* para seguir esperando, o *2* para aplazar y crear una nueva:");
        }

        
        private async Task AvanzarAsync(ConversacionFlotaChat c, int conductorId, int usuarioId)
        {
            c.FechaExpiracion = DateTime.Now.Add(Duracion);

            if (c.VehiculoIdTemp == null)
            {
                c.Paso = "EsperandoPlaca";
                await _context.SaveChangesAsync();
                await ResponderAsync(usuarioId, "🚗 Indica la placa del vehículo (ej: ABC123):");
                return;
            }
            if (string.IsNullOrWhiteSpace(c.DestinoTemp))
            {
                c.Paso = "EsperandoDestino";
                await _context.SaveChangesAsync();
                await ResponderAsync(usuarioId, "📍 Indica el destino del viaje:");
                return;
            }
            if (c.TipoVueltaTemp == null)
            {
                c.Paso = "EsperandoTipoVuelta";
                await _context.SaveChangesAsync();
                await ResponderAsync(usuarioId, "📋 ¿Cuál es el tipo de vuelta? Escribe el número o el nombre:\n\n" + ListaTipos);
                return;
            }

            await CrearAutorizacionAsync(c, conductorId, usuarioId);
        }

        private async Task CrearAutorizacionAsync(ConversacionFlotaChat c, int conductorId, int usuarioId)
        {
           
            if (await VehiculoEnUsoAsync(c.VehiculoIdTemp!.Value))
            {
                c.VehiculoIdTemp = null;
                c.Paso = "EsperandoPlaca";
                await _context.SaveChangesAsync();
                await ResponderAsync(usuarioId, "❌ Ese vehículo acaba de ocuparse en otro viaje. Indica otra placa, o escribe *cancelar* para salir:");
                return;
            }

            var conductor = await _context.Conductores.FindAsync(conductorId);
            var vehiculo = await _context.Vehiculos.FindAsync(c.VehiculoIdTemp.Value);
            var ahora = DateTime.Now;

            _context.Autorizaciones.Add(new Autorizacion
            {
                ConductorId = conductorId,
                VehiculoId = c.VehiculoIdTemp.Value,
                TipoVuelta = c.TipoVueltaTemp!,
                DestinoCompleto = c.DestinoTemp!,
                DescripcionCarga = string.Empty,
                Estado = "Autorizado",
                FechaCreacion = ahora,
                FechaSalidaReal = ahora,
                UsuarioPorteria = "Chat FlotaChat",
                FechaPorteria = ahora
            });
            _context.ConversacionesFlotaChat.Remove(c);
            await _context.SaveChangesAsync();

            await ResponderAsync(usuarioId,
                $"✅ Autorización creada para {conductor?.Nombre ?? "-"}\n" +
                $"🚗 Vehículo: {vehiculo?.Placa ?? "-"}\n" +
                $"📍 Destino: {c.DestinoTemp}\n" +
                $"📋 Tipo: {c.TipoVueltaTemp}\n\n" +
                "🚛 ¡Buen viaje!");
        }

        // ── Utilidades ──────────────────────────────────────────────────────

        private async Task<(Vehiculo? vehiculo, string? error)> BuscarVehiculoLibreAsync(string placa)
        {
            var buscada = placa.ToUpperInvariant().Replace(" ", "").Replace("-", "");
            var vehiculo = await _context.Vehiculos
                .FirstOrDefaultAsync(v => v.Placa.ToUpper().Replace(" ", "").Replace("-", "") == buscada);

            if (vehiculo == null)
                return (null, $"❌ No encontré el vehículo con placa {buscada}. Intenta de nuevo:");

            if (await VehiculoEnUsoAsync(vehiculo.Id))
                return (null, $"❌ El vehículo {vehiculo.Placa} ya está en uso en otro viaje activo. Indica otra placa, o escribe *cancelar* para salir:");

            return (vehiculo, null);
        }

        private Task<bool> VehiculoEnUsoAsync(int vehiculoId) =>
            _context.Autorizaciones.AnyAsync(a => a.VehiculoId == vehiculoId &&
                (a.Estado == "Pendiente" || a.Estado == "Bodega" || a.Estado == "Porteria" ||
                 (a.Estado == "Autorizado" && a.EstadoLlegada == null) ||
                 (a.Estado == "Autorizado" && a.EstadoLlegada == "ReportadaLlegada")));

        private async Task ResponderAsync(int flotaChatUsuarioId, string mensaje)
        {
            var vinculacion = await _context.VinculacionesFlotaChat
                .FirstOrDefaultAsync(v => v.FlotaChatUsuarioId == flotaChatUsuarioId);
            if (vinculacion?.Telefono != null)
                await _mensajeria.EnviarMensajeAsync(vinculacion.Telefono, mensaje);
        }
    }
}