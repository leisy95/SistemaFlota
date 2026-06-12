using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace SistemaFlota
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AutorizacionesController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly AuditoriaService _auditoria;
        private readonly ITwilioService _twilio;

        public AutorizacionesController(
            AppDbContext context,
            AuditoriaService auditoria,
            ITwilioService twilio)
        {
            _context = context;
            _auditoria = auditoria;
            _twilio = twilio;
        }

        private string GetUsuario() =>
            User.FindFirst(ClaimTypes.Name)?.Value ?? "Desconocido";
        private string GetRol() =>
            User.FindFirst(ClaimTypes.Role)?.Value ?? "Desconocido";

        private async Task<Autorizacion?> CargarConRelaciones(int id) =>
            await _context.Autorizaciones
                .Include(a => a.Conductor)
                .Include(a => a.Vehiculo)
                .FirstOrDefaultAsync(a => a.Id == id);

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            try
            {
                var lista = await _context.Autorizaciones
                    .Include(a => a.Conductor)
                    .Include(a => a.Vehiculo)
                    .OrderByDescending(a => a.FechaCreacion)
                    .ToListAsync();
                return Ok(lista);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ ERROR GET Autorizaciones: {ex.Message}");
                return StatusCode(500, ex.InnerException?.Message ?? ex.Message);
            }
        }

        [HttpGet("generar-guia")]
        public async Task<IActionResult> GenerarGuia()
        {
            try
            {
                var total = await _context.Autorizaciones
                    .Where(a => a.NumeroGuia != null && a.NumeroGuia.StartsWith("GI-"))
                    .CountAsync();
                var consecutivo = (total + 1).ToString("D4");
                var fecha = FechaHelper.Ahora().ToString("yyyyMMdd");
                return Ok(new { guia = $"GI-{fecha}-{consecutivo}" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.InnerException?.Message ?? ex.Message);
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var a = await CargarConRelaciones(id);
                if (a == null) return NotFound();
                return Ok(a);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.InnerException?.Message ?? ex.Message);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] CrearAutorizacionDto dto)
        {
            try
            {
                var autorizacion = new Autorizacion
                {
                    ConductorId = dto.ConductorId,
                    VehiculoId = dto.VehiculoId,
                    DestinoCompleto = dto.DestinoCompleto ?? string.Empty,
                    CantidadClientes = dto.CantidadClientes,
                    PesoKilos = dto.PesoKilos,
                    TipoVuelta = dto.TipoVuelta ?? string.Empty,
                    DescripcionCarga = dto.DescripcionCarga ?? string.Empty,
                    NumeroGuia = dto.NumeroGuia,
                    FacturasClientes = dto.FacturasClientes,
                    Estado = "Pendiente",
                    FechaCreacion = FechaHelper.Ahora()
                };
                _context.Autorizaciones.Add(autorizacion);
                await _context.SaveChangesAsync();
                var resultado = await CargarConRelaciones(autorizacion.Id);
                await _auditoria.RegistrarAsync(
                    usuario: GetUsuario(), rol: GetRol(),
                    accion: "Crear", modulo: "Autorizaciones",
                    detalle: $"Autorización creada — Conductor: {resultado?.Conductor?.Nombre ?? "-"}, Vehículo: {resultado?.Vehiculo?.Placa ?? "-"}",
                    registroId: autorizacion.Id
                );
                return Ok(resultado);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ ERROR POST Autorizacion: {ex.Message}\n{ex.InnerException?.Message}");
                return StatusCode(500, ex.InnerException?.Message ?? ex.Message);
            }
        }

        [HttpPost("salida-rapida")]
        public async Task<IActionResult> SalidaRapida([FromBody] SalidaRapidaDto dto)
        {
            try
            {
                var ahora = FechaHelper.Ahora();
                var autorizacion = new Autorizacion
                {
                    ConductorId = dto.ConductorId,
                    VehiculoId = dto.VehiculoId,
                    TipoVuelta = dto.TipoVuelta ?? "Mensajería",
                    DestinoCompleto = dto.DestinoCompleto ?? string.Empty,
                    DescripcionCarga = string.Empty,
                    Estado = "Autorizado",
                    FechaCreacion = ahora,
                    FechaSalidaReal = ahora,
                    UsuarioPorteria = GetUsuario(),
                    FechaPorteria = ahora,
                };
                _context.Autorizaciones.Add(autorizacion);
                await _context.SaveChangesAsync();
                var resultado = await CargarConRelaciones(autorizacion.Id);
                await _auditoria.RegistrarAsync(
                    usuario: GetUsuario(), rol: GetRol(),
                    accion: "SalidaRapida", modulo: "Autorizaciones",
                    detalle: $"Salida rápida — Conductor: {resultado?.Conductor?.Nombre ?? "-"}, Vehículo: {resultado?.Vehiculo?.Placa ?? "-"}",
                    registroId: autorizacion.Id
                );
                if (resultado != null)
                {
                    var hora = ahora.ToString("hh:mm tt");
                    var fecha = ahora.ToString("dd/MM/yyyy");
                    var conductor = resultado.Conductor?.Nombre ?? "-";
                    var placa = resultado.Vehiculo?.Placa ?? "-";
                    var mensaje =
                        $"🚛 *SALIDA EN RUTA*\n━━━━━━━━━━━━━━━━━━\n" +
                        $"👤 Conductor: {conductor}\n🚗 Vehículo: {placa}\n" +
                        $"📍 Destino: {resultado.DestinoCompleto ?? "-"}\n" +
                        $"🕐 Hora salida: {hora} — {fecha}\n━━━━━━━━━━━━━━━━━━";
                    var numerosGrupo = await _context.ContactosNotificacion
                        .Where(c => c.Activo && c.RecibeIncidentes)
                        .Select(c => c.NumeroWhatsApp).ToListAsync();
                    if (numerosGrupo.Any()) await _twilio.EnviarAMultiplesAsync(numerosGrupo, mensaje);
                }
                return Ok(resultado);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ ERROR SalidaRapida: {ex.Message}");
                return StatusCode(500, ex.InnerException?.Message ?? ex.Message);
            }
        }

        [HttpPost("llegada-rapida")]
        public async Task<IActionResult> LlegadaRapida([FromBody] LlegadaRapidaDto dto)
        {
            try
            {
                var ahora = FechaHelper.Ahora();
                var autorizacion = new Autorizacion
                {
                    ConductorId = dto.ConductorId,
                    VehiculoId = dto.VehiculoId,
                    TipoVuelta = dto.TipoVuelta ?? "Mensajería",
                    DestinoCompleto = dto.DestinoCompleto ?? string.Empty,
                    DescripcionCarga = string.Empty,
                    Estado = "Autorizado",
                    FechaCreacion = ahora,
                    FechaSalidaReal = ahora,
                    FechaReporteLlegada = ahora,
                    KilometrajeFinal = dto.KilometrajeFinal,
                    NovedadesViaje = dto.NovedadesViaje,
                    EstadoVehiculoLlegada = dto.EstadoVehiculo ?? "Bueno",
                    EstadoLlegada = "ReportadaLlegada",
                    UsuarioPorteria = GetUsuario(),
                    FechaPorteria = ahora,
                };
                _context.Autorizaciones.Add(autorizacion);
                await _context.SaveChangesAsync();
                var resultado = await CargarConRelaciones(autorizacion.Id);
                await _auditoria.RegistrarAsync(
                    usuario: GetUsuario(), rol: GetRol(),
                    accion: "LlegadaRapida", modulo: "Autorizaciones",
                    detalle: $"Llegada rápida — Conductor: {resultado?.Conductor?.Nombre ?? "-"}, Km: {dto.KilometrajeFinal}",
                    registroId: autorizacion.Id
                );
                if (resultado != null)
                {
                    var hora = ahora.ToString("hh:mm tt");
                    var fecha = ahora.ToString("dd/MM/yyyy");
                    var conductor = resultado.Conductor?.Nombre ?? "-";
                    var placa = resultado.Vehiculo?.Placa ?? "-";
                    var km = dto.KilometrajeFinal?.ToString() ?? "-";
                    var estado = dto.EstadoVehiculo ?? "Bueno";
                    var novedades = string.IsNullOrWhiteSpace(dto.NovedadesViaje) ? "Sin novedades" : dto.NovedadesViaje;
                    var mensaje =
                        $"🏁 *LLEGADA REGISTRADA*\n━━━━━━━━━━━━━━━━━━\n" +
                        $"👤 Conductor: {conductor}\n🚗 Vehículo: {placa}\n" +
                        $"🛣 Km final: {km}\n🔧 Estado vehículo: {estado}\n" +
                        $"📋 Novedades: {novedades}\n🕐 Hora: {hora} — {fecha}\n━━━━━━━━━━━━━━━━━━";
                    var numerosGrupo = await _context.ContactosNotificacion
                        .Where(c => c.Activo && c.RecibeIncidentes)
                        .Select(c => c.NumeroWhatsApp).ToListAsync();
                    if (numerosGrupo.Any()) await _twilio.EnviarAMultiplesAsync(numerosGrupo, mensaje);
                }
                return Ok(resultado);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ ERROR LlegadaRapida: {ex.Message}");
                return StatusCode(500, ex.InnerException?.Message ?? ex.Message);
            }
        }

        // ── EDITAR — con sincronización de trazabilidad ───────────────────────
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] CrearAutorizacionDto dto)
        {
            try
            {
                var a = await _context.Autorizaciones.FindAsync(id);
                if (a == null) return NotFound();

                a.ConductorId = dto.ConductorId;
                a.VehiculoId = dto.VehiculoId;
                a.DestinoCompleto = dto.DestinoCompleto;
                a.CantidadClientes = dto.CantidadClientes;
                a.PesoKilos = dto.PesoKilos;
                a.TipoVuelta = dto.TipoVuelta;
                a.DescripcionCarga = dto.DescripcionCarga;
                a.NumeroGuia = dto.NumeroGuia;
                a.FacturasClientes = dto.FacturasClientes;
                await _context.SaveChangesAsync();

                // ── Sincronizar trazabilidad: solo facturas con número real ──
                if (!string.IsNullOrWhiteSpace(dto.FacturasClientes))
                {
                    try
                    {
                        var resultado = await CargarConRelaciones(id);
                        var conductor = resultado?.Conductor?.Nombre ?? "-";
                        var vehiculo = resultado?.Vehiculo?.Placa ?? "-";
                        var guia = resultado?.NumeroGuia;

                        var facturas = System.Text.Json.JsonSerializer.Deserialize<List<FacturaClienteDto>>(
                            dto.FacturasClientes,
                            new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                        );

                        if (facturas != null && facturas.Count > 0)
                        {
                            foreach (var f in facturas)
                            {
                                // ── Solo facturas con número real ──────────
                                if (string.IsNullOrWhiteSpace(f.FacturaRemision) ||
                                    f.FacturaRemision.Trim() == "-")
                                    continue;

                                var existe = await _context.TrazabilidadFacturas
                                    .AnyAsync(t => t.AutorizacionId == id &&
                                                   t.FacturaRemision == f.FacturaRemision);

                                if (!existe)
                                {
                                    _context.TrazabilidadFacturas.Add(new TrazabilidadFactura
                                    {
                                        AutorizacionId = id,
                                        FechaRegistro = FechaHelper.Ahora(),
                                        FacturaRemision = f.FacturaRemision,
                                        Cliente = f.Cliente ?? "-",
                                        Conductor = conductor,
                                        Vehiculo = vehiculo,
                                        Guia = guia,
                                        PesoKilos = f.PesoKilos,
                                        Estado = "Pendiente"
                                    });
                                    Console.WriteLine($"✅ Trazabilidad nueva factura: {f.FacturaRemision} — Autorización #{id}");
                                }
                            }
                            await _context.SaveChangesAsync();
                        }
                    }
                    catch (Exception exTraz)
                    {
                        Console.WriteLine($"⚠️ Error sincronizando trazabilidad en edición: {exTraz.Message}");
                    }
                }

                await _auditoria.RegistrarAsync(
                    usuario: GetUsuario(), rol: GetRol(),
                    accion: "Editar", modulo: "Autorizaciones",
                    detalle: $"Autorización #{id} editada por: {GetUsuario()}",
                    registroId: id
                );

                return Ok(await CargarConRelaciones(id));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.InnerException?.Message ?? ex.Message);
            }
        }

        // ── FIRMAR FACTURACIÓN + TRAZABILIDAD AUTO ────────────────────────────
        [HttpPut("{id}/facturacion")]
        public async Task<IActionResult> FirmarFacturacion(int id, [FromBody] FirmaDto dto)
        {
            try
            {
                var a = await _context.Autorizaciones.FindAsync(id);
                if (a == null) return NotFound();
                a.FirmaFacturacion = dto.Firma;
                a.UsuarioFacturacion = dto.Usuario;
                a.ObservacionFacturacion = dto.Observacion;
                a.FechaFacturacion = FechaHelper.Ahora();
                a.Estado = "Bodega";
                await _context.SaveChangesAsync();

                var resultado = await CargarConRelaciones(id);
                if (resultado != null)
                {
                    var conductor = resultado.Conductor?.Nombre ?? "-";
                    var vehiculo = resultado.Vehiculo?.Placa ?? "-";
                    var guia = resultado.NumeroGuia;

                    Console.WriteLine($"📋 FirmarFacturacion #{id} — FacturasClientes: {resultado.FacturasClientes ?? "null"}");

                    if (!string.IsNullOrWhiteSpace(resultado.FacturasClientes))
                    {
                        try
                        {
                            var facturas = System.Text.Json.JsonSerializer.Deserialize<List<FacturaClienteDto>>(
                                resultado.FacturasClientes,
                                new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                            );

                            if (facturas != null && facturas.Count > 0)
                            {
                                foreach (var f in facturas)
                                {
                                    // ── Solo facturas con número real ──────
                                    if (string.IsNullOrWhiteSpace(f.FacturaRemision) ||
                                        f.FacturaRemision.Trim() == "-")
                                        continue;

                                    var existe = await _context.TrazabilidadFacturas
                                        .AnyAsync(t => t.AutorizacionId == id &&
                                                       t.FacturaRemision == f.FacturaRemision);

                                    if (!existe)
                                    {
                                        _context.TrazabilidadFacturas.Add(new TrazabilidadFactura
                                        {
                                            AutorizacionId = id,
                                            FechaRegistro = FechaHelper.Ahora(),
                                            FacturaRemision = f.FacturaRemision,
                                            Cliente = f.Cliente ?? "-",
                                            Conductor = conductor,
                                            Vehiculo = vehiculo,
                                            Guia = guia,
                                            PesoKilos = f.PesoKilos,
                                            Estado = "Pendiente"
                                        });
                                    }
                                }
                                await _context.SaveChangesAsync();
                                Console.WriteLine($"✅ Trazabilidad creada desde facturas para autorización #{id}");
                            }
                        }
                        catch (Exception exFact)
                        {
                            Console.WriteLine($"⚠️ Error creando trazabilidad desde facturas: {exFact.Message}");
                        }
                    }
                    // ── Si no hay facturas NO se crea registro en trazabilidad ──
                    // (eliminado el bloque else que creaba registro con "-")
                }

                await _auditoria.RegistrarAsync(
                    usuario: GetUsuario(), rol: GetRol(),
                    accion: "Firmar", modulo: "Autorizaciones",
                    detalle: $"Firma Facturación — #{id}, por: {dto.Usuario}",
                    registroId: id
                );
                return Ok(await CargarConRelaciones(id));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ ERROR FirmarFacturacion: {ex.Message}\n{ex.InnerException?.Message}");
                return StatusCode(500, ex.InnerException?.Message ?? ex.Message);
            }
        }

        [HttpPut("{id}/bodega")]
        public async Task<IActionResult> FirmarBodega(int id, [FromBody] FirmaDto dto)
        {
            try
            {
                var a = await _context.Autorizaciones.FindAsync(id);
                if (a == null) return NotFound();
                a.FirmaBodega = dto.Firma;
                a.UsuarioBodega = dto.Usuario;
                a.ObservacionBodega = dto.Observacion;
                a.FechaBodega = FechaHelper.Ahora();
                a.Estado = "Porteria";
                await _context.SaveChangesAsync();
                await _auditoria.RegistrarAsync(
                    usuario: GetUsuario(), rol: GetRol(),
                    accion: "Firmar", modulo: "Autorizaciones",
                    detalle: $"Firma Bodega — #{id}, por: {dto.Usuario}",
                    registroId: id
                );
                return Ok(await CargarConRelaciones(id));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.InnerException?.Message ?? ex.Message);
            }
        }

        [HttpPut("{id}/porteria")]
        public async Task<IActionResult> FirmarPorteria(int id, [FromBody] FirmaDto dto)
        {
            try
            {
                var a = await _context.Autorizaciones.FindAsync(id);
                if (a == null) return NotFound();
                a.FirmaPorteria = dto.Firma;
                a.UsuarioPorteria = dto.Usuario;
                a.ObservacionPorteria = dto.Observacion;
                a.FechaPorteria = FechaHelper.Ahora();
                a.Estado = "Autorizado";
                a.EstadoLlegada = null;
                await _context.SaveChangesAsync();
                await _auditoria.RegistrarAsync(
                    usuario: GetUsuario(), rol: GetRol(),
                    accion: "Firmar", modulo: "Autorizaciones",
                    detalle: $"Firma Portería Salida — #{id} AUTORIZADO, por: {dto.Usuario}",
                    registroId: id
                );

                var resultado = await CargarConRelaciones(id);
                if (resultado != null)
                {
                    var ahora = FechaHelper.Ahora();
                    var hora = ahora.ToString("hh:mm tt");
                    var fecha = ahora.ToString("dd/MM/yyyy");
                    var conductor = resultado.Conductor?.Nombre ?? "-";
                    var placa = resultado.Vehiculo?.Placa ?? "-";
                    var destino = resultado.DestinoCompleto ?? "-";
                    var tipo = resultado.TipoVuelta ?? "-";
                    var guia = resultado.NumeroGuia ?? "-";

                    var numeroConductor = resultado.Conductor?.Telefono;
                    if (!string.IsNullOrWhiteSpace(numeroConductor))
                    {
                        var mensajeConductor =
                            $"🚚 *SALIDA AUTORIZADA*\n" +
                            $"Hola {conductor.Split(' ')[0]}, tu salida fue autorizada ✅\n" +
                            $"━━━━━━━━━━━━━━━━━━\n" +
                            $"🚗 Vehículo: {placa}\n📍 Destino: {destino}\n🔄 Tipo: {tipo}\n" +
                            (guia != "-" ? $"🔖 Guía: {guia}\n" : "") +
                            $"🕐 Hora: {hora} — {fecha}\n✍️ Autorizado por: {dto.Usuario}\n" +
                            $"━━━━━━━━━━━━━━━━━━\n🛣️ ¡Buen viaje!";
                        await _twilio.EnviarMensajeAsync(numeroConductor, mensajeConductor);
                    }

                    var numerosGrupo = await _context.ContactosNotificacion
                        .Where(c => c.Activo && c.RecibeIncidentes)
                        .Select(c => c.NumeroWhatsApp).ToListAsync();
                    if (numerosGrupo.Any())
                    {
                        var mensajeGrupo =
                            $"🚚 *SALIDA AUTORIZADA*\n━━━━━━━━━━━━━━━━━━\n" +
                            $"👤 Conductor: {conductor}\n🚗 Vehículo: {placa}\n" +
                            $"📍 Destino: {destino}\n🔄 Tipo: {tipo}\n" +
                            (guia != "-" ? $"🔖 Guía: {guia}\n" : "") +
                            $"🚪 Portería: {dto.Usuario}\n🕐 Hora: {hora} — {fecha}\n━━━━━━━━━━━━━━━━━━";
                        await _twilio.EnviarAMultiplesAsync(numerosGrupo, mensajeGrupo);
                    }
                }
                return Ok(resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.InnerException?.Message ?? ex.Message);
            }
        }

        [HttpPut("{id}/reportar-llegada")]
        public async Task<IActionResult> ReportarLlegada(int id, [FromBody] LlegadaConductorDto dto)
        {
            try
            {
                var a = await _context.Autorizaciones.FindAsync(id);
                if (a == null) return NotFound();
                if (a.Estado != "Autorizado")
                    return BadRequest("Solo se puede reportar llegada de autorizaciones en estado Autorizado");
                if (a.EstadoLlegada == "ReportadaLlegada" || a.EstadoLlegada == "Completada")
                    return BadRequest("La llegada ya fue reportada");

                a.FechaReporteLlegada = FechaHelper.Ahora();
                a.KilometrajeFinal = dto.KilometrajeFinal;
                a.NovedadesViaje = dto.NovedadesViaje;
                a.EstadoVehiculoLlegada = dto.EstadoVehiculo;
                a.EstadoLlegada = "ReportadaLlegada";
                await _context.SaveChangesAsync();

                await _auditoria.RegistrarAsync(
                    usuario: GetUsuario(), rol: GetRol(),
                    accion: "ReportarLlegada", modulo: "Autorizaciones",
                    detalle: $"Llegada reportada — #{id}, Km: {dto.KilometrajeFinal}, Estado: {dto.EstadoVehiculo}",
                    registroId: id
                );

                var resultado = await CargarConRelaciones(id);
                if (resultado != null)
                {
                    var ahora = FechaHelper.Ahora();
                    var hora = ahora.ToString("hh:mm tt");
                    var fecha = ahora.ToString("dd/MM/yyyy");
                    var conductor = resultado.Conductor?.Nombre ?? "-";
                    var placa = resultado.Vehiculo?.Placa ?? "-";
                    var km = dto.KilometrajeFinal?.ToString() ?? "-";
                    var estado = dto.EstadoVehiculo ?? "Bueno";
                    var novedades = string.IsNullOrWhiteSpace(dto.NovedadesViaje) ? "Sin novedades" : dto.NovedadesViaje;

                    var mensajeGrupo =
                        $"🏁 *LLEGADA REPORTADA*\n━━━━━━━━━━━━━━━━━━\n" +
                        $"👤 Conductor: {conductor}\n🚗 Vehículo: {placa}\n" +
                        $"🛣 Km final: {km}\n🔧 Estado vehículo: {estado}\n" +
                        $"📋 Novedades: {novedades}\n🕐 Hora: {hora} — {fecha}\n━━━━━━━━━━━━━━━━━━";
                    var numerosGrupo = await _context.ContactosNotificacion
                        .Where(c => c.Activo && c.RecibeIncidentes)
                        .Select(c => c.NumeroWhatsApp).ToListAsync();
                    if (numerosGrupo.Any()) await _twilio.EnviarAMultiplesAsync(numerosGrupo, mensajeGrupo);

                    var numeroConductor = resultado.Conductor?.Telefono;
                    if (!string.IsNullOrWhiteSpace(numeroConductor))
                    {
                        var mensajeConfirmacion =
                            $"✅ *Llegada registrada*\n" +
                            $"Hola {conductor.Split(' ')[0]}, tu llegada fue registrada correctamente.\n" +
                            $"🕐 Hora: {hora} — {fecha}\n¡Gracias por el reporte!";
                        await _twilio.EnviarMensajeAsync(numeroConductor, mensajeConfirmacion);
                    }
                }
                return Ok(resultado);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ ERROR ReportarLlegada: {ex.Message}");
                return StatusCode(500, ex.InnerException?.Message ?? ex.Message);
            }
        }

        [HttpPut("{id}/confirmar-llegada")]
        public async Task<IActionResult> ConfirmarLlegada(int id, [FromBody] FirmaDto dto)
        {
            try
            {
                var a = await _context.Autorizaciones.FindAsync(id);
                if (a == null) return NotFound();
                if (a.EstadoLlegada != "ReportadaLlegada")
                    return BadRequest("El conductor aún no ha reportado la llegada");
                a.FechaConfirmacionLlegada = FechaHelper.Ahora();
                a.UsuarioPorteriaLlegada = dto.Usuario;
                a.ObservacionPorteriaLlegada = dto.Observacion;
                a.FirmaPorteriaLlegada = dto.Firma;
                a.EstadoLlegada = "Completada";
                await _context.SaveChangesAsync();
                await _auditoria.RegistrarAsync(
                    usuario: GetUsuario(), rol: GetRol(),
                    accion: "ConfirmarLlegada", modulo: "Autorizaciones",
                    detalle: $"Llegada confirmada por portería — #{id}, por: {dto.Usuario}",
                    registroId: id
                );
                return Ok(await CargarConRelaciones(id));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ ERROR ConfirmarLlegada: {ex.Message}");
                return StatusCode(500, ex.InnerException?.Message ?? ex.Message);
            }
        }

        [HttpPut("{id}/confirmar-salida")]
        public async Task<IActionResult> ConfirmarSalida(int id)
        {
            try
            {
                var a = await _context.Autorizaciones.FindAsync(id);
                if (a == null) return NotFound();
                if (a.Estado != "Autorizado")
                    return BadRequest("Solo se puede confirmar salida de autorizaciones en estado Autorizado");
                if (a.FechaSalidaReal != null)
                    return BadRequest("La salida ya fue confirmada");

                a.FechaSalidaReal = FechaHelper.Ahora();
                await _context.SaveChangesAsync();
                await _auditoria.RegistrarAsync(
                    usuario: GetUsuario(), rol: GetRol(),
                    accion: "ConfirmarSalida", modulo: "Autorizaciones",
                    detalle: $"Salida en ruta confirmada — #{id}, hora: {a.FechaSalidaReal:hh:mm tt dd/MM/yyyy}",
                    registroId: id
                );

                var resultado = await CargarConRelaciones(id);
                if (resultado != null)
                {
                    var ahora = a.FechaSalidaReal.Value;
                    var hora = ahora.ToString("hh:mm tt");
                    var fecha = ahora.ToString("dd/MM/yyyy");
                    var conductor = resultado.Conductor?.Nombre ?? "-";
                    var placa = resultado.Vehiculo?.Placa ?? "-";
                    var mensaje =
                        $"🚛 *SALIDA EN RUTA*\n━━━━━━━━━━━━━━━━━━\n" +
                        $"👤 Conductor: {conductor}\n🚗 Vehículo: {placa}\n" +
                        $"📍 Destino: {resultado.DestinoCompleto ?? "-"}\n" +
                        $"🕐 Hora salida real: {hora} — {fecha}\n━━━━━━━━━━━━━━━━━━";
                    var numerosGrupo = await _context.ContactosNotificacion
                        .Where(c => c.Activo && c.RecibeIncidentes)
                        .Select(c => c.NumeroWhatsApp).ToListAsync();
                    if (numerosGrupo.Any()) await _twilio.EnviarAMultiplesAsync(numerosGrupo, mensaje);
                }
                return Ok(resultado);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ ERROR ConfirmarSalida: {ex.Message}");
                return StatusCode(500, ex.InnerException?.Message ?? ex.Message);
            }
        }

        [HttpPut("{id}/rechazar")]
        public async Task<IActionResult> Rechazar(int id, [FromBody] FirmaDto dto)
        {
            try
            {
                var a = await _context.Autorizaciones.FindAsync(id);
                if (a == null) return NotFound();
                a.Estado = "Rechazado";
                await _context.SaveChangesAsync();
                await _auditoria.RegistrarAsync(
                    usuario: GetUsuario(), rol: GetRol(),
                    accion: "Rechazar", modulo: "Autorizaciones",
                    detalle: $"Autorización #{id} RECHAZADA",
                    registroId: id
                );
                return Ok(await CargarConRelaciones(id));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.InnerException?.Message ?? ex.Message);
            }
        }
    }

    // ── DTOs ──────────────────────────────────────────────────────────────────
    public class CrearAutorizacionDto
    {
        public int ConductorId { get; set; }
        public int VehiculoId { get; set; }
        public string DestinoCompleto { get; set; } = string.Empty;
        public int CantidadClientes { get; set; }
        public decimal PesoKilos { get; set; }
        public string TipoVuelta { get; set; } = string.Empty;
        public string DescripcionCarga { get; set; } = string.Empty;
        public string? NumeroGuia { get; set; }
        public string? FacturasClientes { get; set; }
    }

    public class FirmaDto
    {
        public string Firma { get; set; } = string.Empty;
        public string Usuario { get; set; } = string.Empty;
        public string? Observacion { get; set; }
    }

    public class LlegadaConductorDto
    {
        public int? KilometrajeFinal { get; set; }
        public string? NovedadesViaje { get; set; }
        public string EstadoVehiculo { get; set; } = "Bueno";
    }

    public class FacturaClienteDto
    {
        public string? FacturaRemision { get; set; }
        public string? Cliente { get; set; }
        public decimal? PesoKilos { get; set; }
    }

    public class SalidaRapidaDto
    {
        public int ConductorId { get; set; }
        public int VehiculoId { get; set; }
        public string? TipoVuelta { get; set; }
        public string? DestinoCompleto { get; set; }
    }

    public class LlegadaRapidaDto
    {
        public int ConductorId { get; set; }
        public int VehiculoId { get; set; }
        public string? TipoVuelta { get; set; }
        public string? DestinoCompleto { get; set; }
        public int? KilometrajeFinal { get; set; }
        public string? NovedadesViaje { get; set; }
        public string? EstadoVehiculo { get; set; }
    }
}