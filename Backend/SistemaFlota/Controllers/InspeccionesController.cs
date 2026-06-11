using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Text.Json;
using System.Security.Claims;

namespace SistemaFlota
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class InspeccionesController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly AuditoriaService _auditoria;
        private readonly ITwilioService _twilio;

        public InspeccionesController(
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

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var lista = await _context.Inspecciones
                .Include(i => i.Vehiculo)
                .Include(i => i.Conductor)
                .OrderByDescending(i => i.Fecha)
                .ToListAsync();
            return Ok(lista);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetDetalle(int id)
        {
            var inspeccion = await _context.Inspecciones
                .Include(i => i.Conductor)
                .Include(i => i.Vehiculo)
                .FirstOrDefaultAsync(i => i.Id == id);
            if (inspeccion == null) return NotFound();

            var detalles = await _context.InspeccionDetalles
                .Include(d => d.ChecklistItem)
                .Where(d => d.InspeccionId == id)
                .Select(d => new
                {
                    d.Id,
                    d.ChecklistItemId,
                    NombreItem = d.ChecklistItem != null
                        ? d.ChecklistItem.Descripcion
                        : (d.DescripcionItem ?? d.ChecklistItemId.ToString()),
                    d.Estado,
                    d.Observacion,
                    d.FotoEvidencia
                })
                .ToListAsync();

            bool tieneRechazado = detalles.Any(d => d.Estado == "No conforme");
            string estadoGeneral = tieneRechazado ? "RECHAZADO" : "APROBADO";

            return Ok(new
            {
                inspeccion.Id,
                inspeccion.Fecha,
                inspeccion.Kilometraje,
                inspeccion.FotoOdometro,
                inspeccion.FirmaCondutor,
                EstadoGeneral = estadoGeneral,
                Conductor = new { inspeccion.Conductor!.Id, inspeccion.Conductor.Nombre },
                Vehiculo = new { inspeccion.Vehiculo!.Id, inspeccion.Vehiculo.Placa },
                Detalles = detalles
            });
        }

        [HttpPost]
        public async Task<IActionResult> Post(
            [FromForm] int VehiculoId,
            [FromForm] int ConductorId,
            [FromForm] int Kilometraje,
            [FromForm] string Checklist,
            IFormFile? FotoOdometro,
            IFormFile? FirmaCondutor,
            [FromForm] List<IFormFile> Evidencias,
            [FromForm] List<int> EvidenciaIndices)
        {
            try
            {
                var carpeta = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/inspecciones");
                if (!Directory.Exists(carpeta)) Directory.CreateDirectory(carpeta);

                string? nombreOdometro = null;
                if (FotoOdometro != null)
                {
                    nombreOdometro = Guid.NewGuid().ToString() + Path.GetExtension(FotoOdometro.FileName);
                    using var s1 = new FileStream(Path.Combine(carpeta, nombreOdometro), FileMode.Create);
                    await FotoOdometro.CopyToAsync(s1);
                }

                string? nombreFirma = null;
                if (FirmaCondutor != null)
                {
                    nombreFirma = Guid.NewGuid().ToString() + Path.GetExtension(FirmaCondutor.FileName);
                    using var s2 = new FileStream(Path.Combine(carpeta, nombreFirma), FileMode.Create);
                    await FirmaCondutor.CopyToAsync(s2);
                }

                var inspeccion = new Inspeccion
                {
                    Fecha = DateTime.Now,
                    VehiculoId = VehiculoId,
                    ConductorId = ConductorId,
                    Kilometraje = Kilometraje,
                    FotoOdometro = nombreOdometro,
                    FirmaCondutor = nombreFirma
                };

                _context.Inspecciones.Add(inspeccion);
                await _context.SaveChangesAsync();

                var opciones = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var items = JsonSerializer.Deserialize<List<ChecklistGuardar>>(Checklist, opciones);
                var itemsNoConformes = new List<string>();

                if (items != null)
                {
                    foreach (var item in items.Select((v, i) => new { v, i }))
                    {
                        string? nombreEvidencia = null;
                        var idx = EvidenciaIndices.FindIndex(x => x == item.i);
                        if (idx >= 0 && idx < Evidencias.Count)
                        {
                            var archivo = Evidencias[idx];
                            nombreEvidencia = Guid.NewGuid().ToString() + Path.GetExtension(archivo.FileName);
                            using var s3 = new FileStream(Path.Combine(carpeta, nombreEvidencia), FileMode.Create);
                            await archivo.CopyToAsync(s3);
                        }

                        // ChecklistItemId nullable — acepta id válido o null
                        int? checklistItemId = item.v.id > 0 ? item.v.id : null;

                        _context.InspeccionDetalles.Add(new InspeccionDetalle
                        {
                            InspeccionId = inspeccion.Id,
                            ChecklistItemId = checklistItemId,
                            DescripcionItem = item.v.descripcion,
                            Estado = item.v.estado,
                            Observacion = item.v.observacion,
                            FotoEvidencia = nombreEvidencia
                        });

                        if (item.v.estado == "No conforme")
                            itemsNoConformes.Add(
                                !string.IsNullOrWhiteSpace(item.v.observacion)
                                    ? item.v.observacion
                                    : item.v.descripcion ?? $"Ítem #{item.v.id}"
                            );
                    }
                    await _context.SaveChangesAsync();
                }

                var conductor = await _context.Conductores.FindAsync(ConductorId);
                var vehiculo = await _context.Vehiculos.FindAsync(VehiculoId);

                await _auditoria.RegistrarAsync(
                    usuario: GetUsuario(), rol: GetRol(),
                    accion: "Crear", modulo: "Inspecciones",
                    detalle: $"Inspección creada — Conductor: {conductor?.Nombre ?? "-"}, Vehículo: {vehiculo?.Placa ?? "-"}, Km: {Kilometraje}",
                    registroId: inspeccion.Id
                );

                // ── TWILIO ────────────────────────────────────────────────────────
                var hora = DateTime.Now.ToString("hh:mm tt");
                var fecha = DateTime.Now.ToString("dd/MM/yyyy");
                var estadoGeneral = itemsNoConformes.Any() ? "⚠️ RECHAZADA" : "✅ APROBADA";

                var mensajeGrupo =
                    $"📋 *INSPECCIÓN {estadoGeneral}*\n" +
                    $"👤 Conductor: {conductor?.Nombre ?? "-"}\n" +
                    $"🚗 Vehículo: {vehiculo?.Placa ?? "-"}\n" +
                    $"🛣 Km: {Kilometraje}\n" +
                    $"🕐 Hora: {hora} — {fecha}";

                if (itemsNoConformes.Any())
                    mensajeGrupo += $"\n⚠️ No conformes: {string.Join(", ", itemsNoConformes)}";

                var numerosGrupo = await _context.ContactosNotificacion
                    .Where(c => c.Activo && c.RecibeIncidentes)
                    .Select(c => c.NumeroWhatsApp)
                    .ToListAsync();

                if (numerosGrupo.Any())
                    await _twilio.EnviarAMultiplesAsync(numerosGrupo, mensajeGrupo);

                if (itemsNoConformes.Any() && !string.IsNullOrWhiteSpace(conductor?.Telefono))
                {
                    var mensajeConductor =
                        $"⚠️ *INSPECCIÓN CON OBSERVACIONES*\n" +
                        $"Hola {conductor.Nombre.Split(' ')[0]},\n" +
                        $"Tu inspección del vehículo {vehiculo?.Placa ?? "-"} tiene ítems no conformes:\n" +
                        $"• {string.Join("\n• ", itemsNoConformes)}\n" +
                        $"Por favor repórtalo al área de mantenimiento.";
                    await _twilio.EnviarMensajeAsync(conductor.Telefono, mensajeConductor);
                }

                return Ok(inspeccion);
            }
            catch (Exception ex)
            {
                await _auditoria.RegistrarAsync(
                    usuario: GetUsuario(), rol: GetRol(),
                    accion: "Crear", modulo: "Inspecciones",
                    detalle: $"Error: {ex.Message}", resultado: "Fallido"
                );
                return StatusCode(500, ex.InnerException?.Message ?? ex.Message);
            }
        }
    }

    public class ChecklistGuardar
    {
        public int id { get; set; }
        public string estado { get; set; } = string.Empty;
        public string? observacion { get; set; }
        public string? descripcion { get; set; }
    }
}