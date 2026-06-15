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

        // ── Calcula estado general por porcentaje ─────────────────────────────
        private static (string estado, string emoji, double porcentaje) EvaluarInspeccion(
            int totalItems, int noConformes)
        {
            if (totalItems == 0) return ("APTO", "✅", 0);
            double pct = (double)noConformes / totalItems * 100;
            if (pct == 0)
                return ("APTO", "✅", pct);
            if (pct <= 20)
                return ("APTO CON OBSERVACIONES", "⚠️", pct);
            return ("NO APTO", "❌", pct);
        }

        [HttpGet]
        public async Task<IActionResult> Get(
            [FromQuery] int pagina = 1,
            [FromQuery] int porPagina = 20,
            [FromQuery] string? buscar = null,
            [FromQuery] int? conductorId = null,
            [FromQuery] int? vehiculoId = null)
        {
            var query = _context.Inspecciones
                .Include(i => i.Vehiculo)
                .Include(i => i.Conductor)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(buscar))
            {
                var q = buscar.ToLower();
                query = query.Where(i =>
                    i.Conductor!.Nombre.ToLower().Contains(q) ||
                    i.Vehiculo!.Placa.ToLower().Contains(q));
            }

            if (conductorId.HasValue)
                query = query.Where(i => i.ConductorId == conductorId.Value);

            if (vehiculoId.HasValue)
                query = query.Where(i => i.VehiculoId == vehiculoId.Value);

            var total = await query.CountAsync();

            var lista = await query
                .OrderByDescending(i => i.Fecha)
                .Skip((pagina - 1) * porPagina)
                .Take(porPagina)
                .ToListAsync();

            return Ok(new
            {
                data = lista,
                total,
                pagina,
                porPagina,
                totalPaginas = (int)Math.Ceiling((double)total / porPagina)
            });
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

            var noConformes = detalles.Count(d => d.Estado == "No conforme");
            var totalItems = detalles.Count;
            var (estado, emoji, pct) = EvaluarInspeccion(totalItems, noConformes);

            return Ok(new
            {
                inspeccion.Id,
                inspeccion.Fecha,
                inspeccion.Kilometraje,
                inspeccion.FotoOdometro,
                inspeccion.FirmaCondutor,
                EstadoGeneral = estado,
                EmojiEstado = emoji,
                PorcentajeNoConf = Math.Round(pct, 1),
                TotalItems = totalItems,
                TotalNoConformes = noConformes,
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

                        _context.InspeccionDetalles.Add(new InspeccionDetalle
                        {
                            InspeccionId = inspeccion.Id,
                            ChecklistItemId = (item.v.id.HasValue && item.v.id > 0) ? item.v.id : null,
                            DescripcionItem = item.v.descripcion,
                            Estado = item.v.estado ?? string.Empty,
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

                // ── Evaluar resultado por porcentaje ──────────────────────────
                var totalItems = items?.Count ?? 0;
                var noConformes = itemsNoConformes.Count;
                var (estadoGen, emojiGen, pctGen) = EvaluarInspeccion(totalItems, noConformes);
                var pctTexto = $"{Math.Round(pctGen, 1)}%";

                await _auditoria.RegistrarAsync(
                    usuario: GetUsuario(), rol: GetRol(),
                    accion: "Crear", modulo: "Inspecciones",
                    detalle: $"Inspección creada — Conductor: {conductor?.Nombre ?? "-"}, " +
                             $"Vehículo: {vehiculo?.Placa ?? "-"}, Km: {Kilometraje}, " +
                             $"Estado: {estadoGen} ({pctTexto} no conformes)",
                    registroId: inspeccion.Id
                );

                // ── Mensaje WhatsApp ──────────────────────────────────────────
                var hora = DateTime.Now.ToString("hh:mm tt");
                var fecha = DateTime.Now.ToString("dd/MM/yyyy");

                var mensajeGrupo =
                    $"📋 *INSPECCIÓN VEHICULAR*\n" +
                    $"━━━━━━━━━━━━━━━━━━\n" +
                    $"{emojiGen} Estado: *{estadoGen}*\n" +
                    $"📊 No conformes: {noConformes}/{totalItems} ({pctTexto})\n" +
                    $"━━━━━━━━━━━━━━━━━━\n" +
                    $"👤 Conductor: {conductor?.Nombre ?? "-"}\n" +
                    $"🚗 Vehículo: {vehiculo?.Placa ?? "-"}\n" +
                    $"🛣 Km: {Kilometraje}\n" +
                    $"🕐 Hora: {hora} — {fecha}";

                if (itemsNoConformes.Any())
                    mensajeGrupo +=
                        $"\n━━━━━━━━━━━━━━━━━━\n" +
                        $"⚠️ Ítems no conformes:\n" +
                        $"• {string.Join("\n• ", itemsNoConformes)}";

                var numerosGrupo = await _context.ContactosNotificacion
                    .Where(c => c.Activo && c.RecibeIncidentes)
                    .Select(c => c.NumeroWhatsApp)
                    .ToListAsync();

                if (numerosGrupo.Any())
                    await _twilio.EnviarAMultiplesAsync(numerosGrupo, mensajeGrupo);

                // ── Mensaje al conductor si hay no conformes ──────────────────
                if (itemsNoConformes.Any() && !string.IsNullOrWhiteSpace(conductor?.Telefono))
                {
                    var mensajeConductor =
                        $"{emojiGen} *INSPECCIÓN: {estadoGen}*\n" +
                        $"Hola {conductor.Nombre.Split(' ')[0]},\n" +
                        $"Tu inspección del vehículo {vehiculo?.Placa ?? "-"} tiene " +
                        $"{noConformes} ítem(s) no conforme(s) ({pctTexto}):\n" +
                        $"• {string.Join("\n• ", itemsNoConformes)}\n" +
                        $"Por favor repórtalo al área de mantenimiento.";
                    await _twilio.EnviarMensajeAsync(conductor.Telefono, mensajeConductor);
                }

                return Ok(new
                {
                    inspeccion.Id,
                    EstadoGeneral = estadoGen,
                    EmojiEstado = emojiGen,
                    PorcentajeNoConf = Math.Round(pctGen, 1),
                    TotalItems = totalItems,
                    TotalNoConformes = noConformes
                });
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
        public int? id { get; set; }
        public string? estado { get; set; }
        public string? observacion { get; set; }
        public string? descripcion { get; set; }
    }
}