
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using SistemaFlota.DTOs;
using SistemaFlota.DTOs.Flota.Inspecciones;

namespace SistemaFlota.Services.Flota.Inspecciones
{
    public class InspeccionService : IInspeccionService
    {
        private readonly AppDbContext _context;
        private readonly AuditoriaService _auditoria;
        private readonly IMensajeriaService _mensajeria;

        public InspeccionService(
            AppDbContext context,
            AuditoriaService auditoria,
            IMensajeriaService mensajeria)
        {
            _context = context;
            _auditoria = auditoria;
            _mensajeria = mensajeria;
        }

        // EVALUAR ESTADO GENERAL
        private static (string estado, string emoji, double porcentaje)
            EvaluarInspeccion(int totalItems, int noConformes)
        {
            if (totalItems == 0)
                return ("APTO", "", 0);

            double porcentaje = (double)noConformes / totalItems * 100;

            if (porcentaje == 0)
                return ("APTO", "", porcentaje);

            if (porcentaje <= 20)
                return ("APTO CON OBSERVACIONES", "", porcentaje);

            return ("NO APTO", "", porcentaje);
        }

        // UTILIDADES PARA ARCHIVOS
        private static string ObtenerCarpetaInspecciones()
        {
            var carpeta = Path.Combine(
                Directory.GetCurrentDirectory(),
                "wwwroot",
                "inspecciones");

            Directory.CreateDirectory(carpeta);

            return carpeta;
        }

        private static async Task<string?> GuardarArchivoAsync(
            IFormFile? archivo,
            string carpeta)
        {
            if (archivo == null || archivo.Length == 0)
                return null;

            var extension = Path.GetExtension(archivo.FileName);
            var nombreArchivo = $"{Guid.NewGuid()}{extension}";
            var ruta = Path.Combine(carpeta, nombreArchivo);

            await using var stream = new FileStream(
                ruta,
                FileMode.CreateNew,
                FileAccess.Write);

            await archivo.CopyToAsync(stream);

            return nombreArchivo;
        }

        // LISTAR INSPECCIONES

        public async Task<object> ObtenerInspeccionesAsync(
            int pagina = 1,
            int porPagina = 20,
            string? buscar = null,
            int? conductorId = null,
            int? vehiculoId = null,
            DateTime? fechaDesde = null,
            DateTime? fechaHasta = null)
        {
            pagina = Math.Max(1, pagina);
            porPagina = Math.Clamp(porPagina, 1, 100);

            var query = _context.Inspecciones
                .AsNoTracking()
                .Include(i => i.Vehiculo)
                .Include(i => i.Conductor)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(buscar))
            {
                var termino = buscar.Trim().ToLower();

                query = query.Where(i =>
                    (i.Conductor != null &&
                     i.Conductor.Nombre.ToLower().Contains(termino))
                    ||
                    (i.Vehiculo != null &&
                     i.Vehiculo.Placa.ToLower().Contains(termino)));
            }

            if (conductorId.HasValue)
            {
                query = query.Where(
                    i => i.ConductorId == conductorId.Value);
            }

            if (vehiculoId.HasValue)
            {
                query = query.Where(
                    i => i.VehiculoId == vehiculoId.Value);
            }

            // Filtro desde: incluye todo el día inicial.
            if (fechaDesde.HasValue)
            {
                var desde = fechaDesde.Value.Date;
                query = query.Where(i => i.Fecha >= desde);
            }

            // Filtro hasta: incluye todo el día final.
            if (fechaHasta.HasValue)
            {
                var hastaExclusivo = fechaHasta.Value.Date.AddDays(1);
                query = query.Where(i => i.Fecha < hastaExclusivo);
            }

            var total = await query.CountAsync();

            var lista = await query
                .OrderByDescending(i => i.Fecha)
                .Skip((pagina - 1) * porPagina)
                .Take(porPagina)
                .ToListAsync();

            return new
            {
                data = lista,
                total,
                pagina,
                porPagina,
                totalPaginas = (int)Math.Ceiling(
                    (double)total / porPagina)
            };
        }


        // OBTENER DETALLE DE UNA INSPECCIÓN
        public async Task<InspeccionDetalleCompletoResponseDto?>
            ObtenerDetalleAsync(int id)
        {
            var inspeccion = await _context.Inspecciones
                .AsNoTracking()
                .Include(i => i.Conductor)
                .Include(i => i.Vehiculo)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (inspeccion == null)
                return null;

            var detalles = await _context.InspeccionDetalles
                .AsNoTracking()
                .Where(d => d.InspeccionId == id)
                .Select(d => new InspeccionDetalleResponseDto
                {
                    Id = d.Id,
                    ChecklistItemId = d.ChecklistItemId,

                    NombreItem = d.ChecklistItem != null
                        ? d.ChecklistItem.Descripcion
                        : (d.DescripcionItem ??
                           (d.ChecklistItemId.HasValue
                               ? d.ChecklistItemId.Value.ToString()
                               : string.Empty)),

                    Estado = d.Estado,
                    Observacion = d.Observacion,
                    FotoEvidencia = d.FotoEvidencia
                })
                .ToListAsync();

            var totalItems = detalles.Count;

            var noConformes = detalles.Count(
                d => d.Estado == "No conforme");

            var (estado, emoji, porcentaje) =
                EvaluarInspeccion(totalItems, noConformes);

            return new InspeccionDetalleCompletoResponseDto
            {
                Id = inspeccion.Id,
                Fecha = inspeccion.Fecha,
                Kilometraje = inspeccion.Kilometraje,
                FotoOdometro = inspeccion.FotoOdometro,
                FirmaCondutor = inspeccion.FirmaCondutor,

                EstadoGeneral = estado,
                EmojiEstado = emoji,
                PorcentajeNoConf = Math.Round(porcentaje, 1),
                TotalItems = totalItems,
                TotalNoConformes = noConformes,

                Conductor = new ConductorInspeccionDto
                {
                    Id = inspeccion.Conductor!.Id,
                    Nombre = inspeccion.Conductor.Nombre
                },

                Vehiculo = new VehiculoInspeccionDto
                {
                    Id = inspeccion.Vehiculo!.Id,
                    Placa = inspeccion.Vehiculo.Placa
                },

                Detalles = detalles
            };
        }

        // CREAR INSPECCIÓN
        public async Task<InspeccionResponseDto> CrearAsync(
            CrearInspeccionDto dto,
            string usuario,
            string rol)
        {
            try
            {
                // Validar referencias y datos básicos---------

                if (dto.VehiculoId <= 0 ||
                    dto.ConductorId <= 0 ||
                    dto.Kilometraje < 0)
                {
                    throw new ArgumentException(
                        "Los datos de la inspección no son válidos.");
                }

                var vehiculo = await _context.Vehiculos
                    .FindAsync(dto.VehiculoId);

                if (vehiculo == null)
                    throw new ArgumentException(
                        "El vehículo seleccionado no existe.");

                var conductor = await _context.Conductores
                    .FindAsync(dto.ConductorId);

                if (conductor == null)
                    throw new ArgumentException(
                        "El conductor seleccionado no existe.");

                // Procesar checklist
                var opcionesJson = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                var items = JsonSerializer.Deserialize<
                    List<ChecklistGuardar>>(
                        dto.Checklist,
                        opcionesJson) ?? new List<ChecklistGuardar>();

                var itemsNoConformes = new List<string>();

                var carpeta = ObtenerCarpetaInspecciones();

                // Guardar fotografías generales

                var nombreOdometro = await GuardarArchivoAsync(
                    dto.FotoOdometro,
                    carpeta);

                var nombreFirma = await GuardarArchivoAsync(
                    dto.FirmaCondutor,
                    carpeta);

                // Crear inspección principal--------------

                var inspeccion = new Inspeccion
                {
                    Fecha = DateTime.Now,
                    VehiculoId = dto.VehiculoId,
                    ConductorId = dto.ConductorId,
                    Kilometraje = dto.Kilometraje,
                    FotoOdometro = nombreOdometro,
                    FirmaCondutor = nombreFirma
                };

                _context.Inspecciones.Add(inspeccion);

                await _context.SaveChangesAsync();

                // Guardar detalles y evidencias-------------

                for (var i = 0; i < items.Count; i++)
                {
                    var item = items[i];

                    string? nombreEvidencia = null;

                    var indiceEvidencia =
                        dto.EvidenciaIndices.FindIndex(x => x == i);

                    if (indiceEvidencia >= 0 &&
                        indiceEvidencia < dto.Evidencias.Count)
                    {
                        nombreEvidencia = await GuardarArchivoAsync(
                            dto.Evidencias[indiceEvidencia],
                            carpeta);
                    }

                    _context.InspeccionDetalles.Add(
                        new InspeccionDetalle
                        {
                            InspeccionId = inspeccion.Id,

                            ChecklistItemId =
                                item.id.HasValue && item.id.Value > 0
                                    ? item.id
                                    : null,

                            DescripcionItem = item.descripcion,
                            Estado = item.estado ?? string.Empty,
                            Observacion = item.observacion,
                            FotoEvidencia = nombreEvidencia
                        });

                    if (item.estado == "No conforme")
                    {
                        itemsNoConformes.Add(
                            !string.IsNullOrWhiteSpace(item.observacion)
                                ? item.observacion
                                : item.descripcion ??
                                  $"Ítem #{item.id}");
                    }
                }

                if (items.Count > 0)
                    await _context.SaveChangesAsync();

                // Evaluar resultado---------------------

                var totalItems = items.Count;
                var totalNoConformes = itemsNoConformes.Count;

                var (estadoGeneral, emojiEstado, porcentaje) =
                    EvaluarInspeccion(
                        totalItems,
                        totalNoConformes);

                var porcentajeTexto =
                    $"{Math.Round(porcentaje, 1)}%";

                // Registrar auditoría--------------------------

                await _auditoria.RegistrarAsync(
                    usuario: usuario,
                    rol: rol,
                    accion: "Crear",
                    modulo: "Inspecciones",
                    detalle:
                        $"Inspección creada — " +
                        $"Conductor: {conductor.Nombre}, " +
                        $"Vehículo: {vehiculo.Placa}, " +
                        $"Km: {dto.Kilometraje}, " +
                        $"Estado: {estadoGeneral} " +
                        $"({porcentajeTexto} no conformes)",
                    registroId: inspeccion.Id
                );

                // Notificar al grupo de WhatsApp--------------

                var ahora = DateTime.Now;
                var hora = ahora.ToString("hh:mm tt");
                var fecha = ahora.ToString("dd/MM/yyyy");

                var mensajeGrupo =
                    $"INSPECCIÓN VEHICULAR\n" +
                    $"Estado: {estadoGeneral}\n" +
                    $"No conformes: {totalNoConformes}/" +
                    $"{totalItems} ({porcentajeTexto})\n" +
                    $"Conductor: {conductor.Nombre}\n" +
                    $"Vehículo: {vehiculo.Placa}\n" +
                    $"Km: {dto.Kilometraje}\n" +
                    $"Hora: {hora} — {fecha}";

                if (itemsNoConformes.Any())
                {
                    mensajeGrupo +=
                        "\nÍtems no conformes:\n" +
                        string.Join(
                            "\n",
                            itemsNoConformes.Select(x => $"- {x}"));
                }

                var numerosGrupo = await _context.ContactosNotificacion
                    .Where(c => c.Activo && c.RecibeInspecciones)
                    .Select(c => c.NumeroWhatsApp)
                    .ToListAsync();

                try
                {
                    await _mensajeria.EnviarAMultiplesAsync(
                        numerosGrupo,
                        mensajeGrupo,
                        "Inspecciones");

                    inspeccion.WhatsAppEnviado = true;
                }
                catch
                {
                    // La inspección se conserva aunque falle
                    // el envío al grupo.
                    inspeccion.WhatsAppEnviado = false;
                }

                await _context.SaveChangesAsync();

                // Notificar al conductor si hay novedades

                if (itemsNoConformes.Any() &&
                    !string.IsNullOrWhiteSpace(conductor.Telefono))
                {
                    var primerNombre = conductor.Nombre
                        .Split(
                            ' ',
                            StringSplitOptions.RemoveEmptyEntries)
                        .FirstOrDefault() ?? conductor.Nombre;

                    var mensajeConductor =
                        $"INSPECCIÓN: {estadoGeneral}\n" +
                        $"Hola {primerNombre},\n" +
                        $"Tu inspección del vehículo {vehiculo.Placa} " +
                        $"tiene {totalNoConformes} ítem(s) no conforme(s) " +
                        $"({porcentajeTexto}):\n" +
                        string.Join(
                            "\n",
                            itemsNoConformes.Select(x => $"- {x}")) +
                        "\nPor favor repórtalo al área de mantenimiento.";

                    await _mensajeria.EnviarMensajeAsync(
                        conductor.Telefono,
                        mensajeConductor,
                        "Conductores");
                }

                // Devolver respuesta al controlador
                return new InspeccionResponseDto
                {
                    Id = inspeccion.Id,
                    EstadoGeneral = estadoGeneral,
                    EmojiEstado = emojiEstado,
                    PorcentajeNoConf = Math.Round(porcentaje, 1),
                    TotalItems = totalItems,
                    TotalNoConformes = totalNoConformes
                };
            }
            catch (Exception ex)
            {
                await _auditoria.RegistrarAsync(
                    usuario: usuario,
                    rol: rol,
                    accion: "Crear",
                    modulo: "Inspecciones",
                    detalle: $"Error: {ex.Message}",
                    resultado: "Fallido"
                );

                throw;
            }
        }
    }
}