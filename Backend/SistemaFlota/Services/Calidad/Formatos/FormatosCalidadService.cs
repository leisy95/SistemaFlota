
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using SistemaFlota.DTOs;
using SistemaFlota.Models;
using SistemaFlota.Models.Calidad;
using SistemaFlota.Services.Auth;
using SistemaFlota.Services.InformacionKasoft.OrdenesProduccion;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace SistemaFlota.Services.Calidad.Formatos
{
    public class FormatosCalidadService
    {
        private readonly AppDbContext _context;
        private readonly AuditoriaService _auditoria;
        private readonly IMensajeriaService _mensajeria;
        private readonly IOrdenProduccionService _ordenProduccionService;
        private readonly MezclaOrdenService _mezclaOrdenService;
        private readonly IWebHostEnvironment _environment;
        private readonly ICurrentUserService _currentUser;

        private const int MaxFotosRonda = 5;
        private const long MaxBytesFotoRonda = 8 * 1024 * 1024;

        private static readonly string[] ExtensionesFotoRonda =
        {
            ".jpg", ".jpeg", ".png", ".webp"
        };

        public FormatosCalidadService(
            AppDbContext context,
            AuditoriaService auditoria,
            IMensajeriaService mensajeria,
            IOrdenProduccionService ordenProduccionService,
            MezclaOrdenService mezclaOrdenService,
            IWebHostEnvironment environment,
            ICurrentUserService currentUser)
        {
            _context = context;
            _auditoria = auditoria;
            _mensajeria = mensajeria;
            _ordenProduccionService = ordenProduccionService;
            _mezclaOrdenService = mezclaOrdenService;
            _environment = environment;
            _currentUser = currentUser;
        }

        private string ObtenerUsuarioActual()
        {
            return _currentUser.Usuario ?? "Desconocido";
        }

        private string ObtenerRolActual()
        {
            return _currentUser.Rol ?? "Desconocido";
        }

        private int ObtenerIdUsuarioActual()
        {
            return _currentUser.IdUsuario ?? 0;
        }

        // GET: tipos
        public async Task<List<object>> ObtenerTiposAsync()
        {
            return await _context.TiposFormatoCalidad
                .OrderBy(t => t.Codigo)
                .Select(t => (object)new
                {
                    t.Id,
                    t.Codigo,
                    t.Nombre,
                    t.TieneVariablesCriticas
                })
                .ToListAsync();
        }

        // GET: tipos/{codigo}/caracteristicas
        public async Task<object?> ObtenerCaracteristicasAsync(string codigo)
        {
            var tipo = await _context.TiposFormatoCalidad
                .Include(t => t.Caracteristicas)
                .FirstOrDefaultAsync(t => t.Codigo == codigo);

            if (tipo == null)
                return null;

            return new
            {
                tipo.Id,
                tipo.Codigo,
                tipo.Nombre,
                tipo.TieneVariablesCriticas,
                Caracteristicas = tipo.Caracteristicas
                    .OrderBy(c => c.Orden)
                    .Select(c => new
                    {
                        c.Id,
                        c.Orden,
                        c.Descripcion
                    })
            };
        }

        // GET: registros
        public async Task<(
            bool TipoEncontrado,
            List<RegistroFormatoCalidad> Registros)>
            ObtenerRegistrosAsync(
                string codigo,
                DateTime? desde,
                DateTime? hasta,
                string? op)
        {
            var tipo = await _context.TiposFormatoCalidad
                .FirstOrDefaultAsync(t => t.Codigo == codigo);

            if (tipo == null)
                return (false, new List<RegistroFormatoCalidad>());

            var query = _context.RegistrosFormatoCalidad
                .Where(r => r.TipoFormatoId == tipo.Id);

            if (desde.HasValue)
                query = query.Where(r => r.Fecha >= desde.Value);

            if (hasta.HasValue)
                query = query.Where(r => r.Fecha <= hasta.Value.AddDays(1));

            if (!string.IsNullOrWhiteSpace(op))
                query = query.Where(r => r.OrdenProduccion.Contains(op));

            var registros = await query
                .OrderByDescending(r => r.Fecha)
                .ToListAsync();

            return (true, registros);
        }

        // POST: registros
        public async Task<(
            RegistroFormatoCalidad? Registro,
            int StatusCode,
            string? Mensaje)>
            CrearAsync(RegistroFormatoCalidadDto dto)
        {
            var usuario = ObtenerUsuarioActual();
            var rol = ObtenerRolActual();
            var usuarioId = ObtenerIdUsuarioActual();

            var registro = new RegistroFormatoCalidad
            {
                TipoFormatoId = dto.TipoFormatoId,
                OrdenProduccion = dto.OrdenProduccion,
                Cliente = dto.Cliente,
                Referencia = dto.Referencia,
                Operarios = dto.Operarios,
                Hora = dto.Hora,
                Maquina = dto.Maquina,
                VariablesCriticasJson = dto.VariablesCriticasJson,
                ResultadosJson = dto.ResultadosJson,
                Estado = "PendienteLiberacion"
            };

            DateTime? fechaManual = null;

            if (dto.FechaRealProduccion.HasValue)
            {
                var cuentasMaquina = new[]
                {
                    "Extrusion",
                    "Precorte",
                    "Impresion",
                    "Sellado"
                };

                var esCuentaMaquina = cuentasMaquina.Contains(
                    usuario,
                    StringComparer.OrdinalIgnoreCase);

                var puedeEditar = rol == "Admin"
                    || await _context.UsuarioPermisos.AnyAsync(p =>
                        p.UsuarioId == usuarioId
                        && p.Modulo == "calidad-formatos"
                        && p.PuedeEditar);

                if (esCuentaMaquina || !puedeEditar)
                {
                    return (
                        null,
                        StatusCodes.Status403Forbidden,
                        "No tiene permiso para fijar la fecha real de producción"
                    );
                }

                var fechaReal = dto.FechaRealProduccion.Value.Date;

                if (fechaReal > DateTime.Now.Date)
                {
                    return (
                        null,
                        StatusCodes.Status400BadRequest,
                        "La fecha real de producción no puede ser futura"
                    );
                }

                registro.Fecha = fechaReal;
                fechaManual = fechaReal;
            }

            _context.RegistrosFormatoCalidad.Add(registro);
            await _context.SaveChangesAsync();

            if (fechaManual.HasValue)
            {
                await _auditoria.RegistrarAsync(
                    usuario,
                    rol,
                    "Fecha-Manual",
                    "FormatosCalidad",
                    $"OP {registro.OrdenProduccion}: fecha real de producción " +
                    $"{fechaManual:dd/MM/yyyy}, registrada el " +
                    $"{DateTime.Now:dd/MM/yyyy HH:mm}",
                    registro.Id);
            }

            await _auditoria.RegistrarAsync(
                usuario,
                rol,
                "Crear",
                "FormatosCalidad",
                $"Registro OP: {dto.OrdenProduccion} - Tipo: {dto.TipoFormatoId}",
                registro.Id);

            var tipo = await _context.TiposFormatoCalidad
                .FindAsync(dto.TipoFormatoId);

            var contactos = await _context.ContactosNotificacion
                .Where(c => c.Activo && c.RecibeLiberaciones)
                .Select(c => c.NumeroWhatsApp)
                .ToListAsync();

            if (contactos.Any())
            {
                var mensaje =
                    $"⚠️ *PENDIENTE DE LIBERACIÓN*\n\n" +
                    $"Formato: {tipo?.Nombre} ({tipo?.Codigo})\n" +
                    $"Orden: {dto.OrdenProduccion}\n" +
                    $"Cliente: {dto.Cliente ?? "-"}\n" +
                    $"Registrado por: {usuario}\n\n" +
                    "Por favor revisa y libera el producto en el sistema.";

                await _mensajeria.EnviarAMultiplesAsync(
                    contactos,
                    mensaje,
                    "Liberaciones");
            }

            return (registro, StatusCodes.Status200OK, null);
        }

        // PUT: registros/{id}
        public async Task<RegistroFormatoCalidad?> ActualizarAsync(
            int id,
            RegistroFormatoCalidadDto dto)
        {
            var registro = await _context.RegistrosFormatoCalidad
                .FindAsync(id);

            if (registro == null)
                return null;

            registro.OrdenProduccion = dto.OrdenProduccion;
            registro.Cliente = dto.Cliente;
            registro.Referencia = dto.Referencia;
            registro.Operarios = dto.Operarios;
            registro.Hora = dto.Hora;
            registro.Maquina = dto.Maquina;
            registro.VariablesCriticasJson = dto.VariablesCriticasJson;
            registro.ResultadosJson = dto.ResultadosJson;

            await _context.SaveChangesAsync();

            return registro;
        }

        // PUT: registros/{id}/liberar
        public async Task<RegistroFormatoCalidad?> LiberarAsync(
            int id,
            LiberarFormatoDto dto)
        {
            var usuario = ObtenerUsuarioActual();
            var rol = ObtenerRolActual();

            var registro = await _context.RegistrosFormatoCalidad
                .FindAsync(id);

            if (registro == null)
                return null;

            registro.PuedeLiberarse = dto.PuedeLiberarse;
            registro.ExplicacionNoLiberado = dto.ExplicacionNoLiberado;
            registro.FirmaDigital = dto.FirmaDigital;
            registro.CargoFirma = dto.CargoFirma;
            registro.ProduccionKgHora = dto.ProduccionKgHora;
            registro.DesperdicioTotalKg = dto.DesperdicioTotalKg;
            registro.RevisadoPor = usuario;
            registro.FechaRevision = DateTime.Now;
            registro.Estado = "Liberado";

            await _context.SaveChangesAsync();

            await _auditoria.RegistrarAsync(
                usuario,
                rol,
                "Liberar",
                "FormatosCalidad",
                $"Liberación OP: {registro.OrdenProduccion} - Resultado: " +
                $"{(dto.PuedeLiberarse == true ? "SI" : "NO")}",
                registro.Id);

            return registro;
        }

        // GET: registros/op/{op}
        public async Task<RegistroFormatoCalidad?> ObtenerPorOPAsync(
            string op,
            int tipoFormatoId)
        {
            return await _context.RegistrosFormatoCalidad
                .Where(r =>
                    r.OrdenProduccion == op &&
                    r.TipoFormatoId == tipoFormatoId)
                .OrderByDescending(r => r.Fecha)
                .FirstOrDefaultAsync();
        }

        // GET: mezcla/{op}
        public async Task<List<MezclaOrdenItem>> ObtenerMezclaAsync(string op)
        {
            try
            {
                return await _mezclaOrdenService.ObtenerAsync(op);
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    $"Error consultando la mezcla de la OP {op}: {ex.Message}");

                return new List<MezclaOrdenItem>();
            }
        }

        // DELETE: registros/{id}
        public async Task<bool> EliminarAsync(int id)
        {
            var registro = await _context.RegistrosFormatoCalidad
                .FindAsync(id);

            if (registro == null)
                return false;

            _context.RegistrosFormatoCalidad.Remove(registro);
            await _context.SaveChangesAsync();

            return true;
        }

        // GET: desperdicio-orden
        public async Task<(
            decimal TotalDesperdicio,
            int TotalRegistros)>
            ObtenerDesperdicioPorOrdenAsync(
                string op,
                int tipoFormatoId)
        {
            var registros = await _context.RegistrosFormatoCalidad
                .Where(r =>
                    r.OrdenProduccion == op &&
                    r.TipoFormatoId == tipoFormatoId)
                .ToListAsync();

            decimal totalDesperdicio = 0;

            foreach (var registro in registros)
            {
                totalDesperdicio += ObtenerDesperdicioDesdeJson(
                    registro.ResultadosJson);
            }

            return (totalDesperdicio, registros.Count);
        }

        // GET: mejor-rendimiento

        public async Task<(int StatusCode, object Resultado)>
            ObtenerMejorRendimientoAsync(string texto, string? maquina)
        {
            var tipoExtrusion = await _context.TiposFormatoCalidad
                .FirstOrDefaultAsync(t => t.Codigo == "F-GC-004");

            if (tipoExtrusion == null)
            {
                return (
                    StatusCodes.Status404NotFound,
                    new { mensaje = "Tipo Extrusión no configurado" }
                );
            }

            var referenciasPorDescripcion =
                await _ordenProduccionService
                    .BuscarReferenciasPorDescripcionAsync(texto);

            var query = _context.RegistrosFormatoCalidad
                .Where(r =>
                    r.TipoFormatoId == tipoExtrusion.Id &&
                    r.ProduccionKgHora != null &&
                    (
                        (r.OrdenProduccion != null && r.OrdenProduccion.Contains(texto)) ||
                        (r.Referencia != null && r.Referencia.Contains(texto)) ||
                        (r.Cliente != null && r.Cliente.Contains(texto)) ||
                        (r.Referencia != null &&
                         referenciasPorDescripcion.Contains(r.Referencia))
                    ));

            if (!string.IsNullOrWhiteSpace(maquina))
                query = query.Where(r => r.Maquina == maquina);

            var registros = await query.ToListAsync();

            if (!registros.Any())
            {
                return (
                    StatusCodes.Status200OK,
                    new
                    {
                        mensaje = "No hay registros históricos para esta búsqueda",
                        resultados = new List<object>()
                    }
                );
            }

            var referencias = registros
                .Where(r => !string.IsNullOrWhiteSpace(r.Referencia))
                .Select(r => r.Referencia!)
                .Distinct()
                .ToList();

            var descripciones = await _ordenProduccionService
                .ObtenerDescripcionesReferenciasAsync(referencias);

            var fijado = registros.FirstOrDefault(r => r.EsMejorFijado);

            var calculados = registros.Select(r =>
            {
                var kgHora = decimal.TryParse(
                    r.ProduccionKgHora, out var kg) ? kg : 0;

                var desperdicioTotal = r.DesperdicioTotalKg ?? 0m;

                return new
                {
                    r.Id,
                    r.OrdenProduccion,
                    r.Referencia,
                    Descripcion = r.Referencia != null &&
                                  descripciones.TryGetValue(
                                      r.Referencia, out var descripcion)
                        ? descripcion
                        : "",
                    r.Fecha,
                    r.Maquina,
                    KgHora = kgHora,
                    DesperdicioTotal = desperdicioTotal,
                    r.VariablesCriticasJson
                };
            }).ToList();

            var maxKgHora = calculados.Max(c => c.KgHora);
            var maxDesperdicio = calculados.Max(c => c.DesperdicioTotal);

            var conPuntaje = calculados
                .Select(c => new
                {
                    c.Id,
                    c.OrdenProduccion,
                    c.Referencia,
                    c.Descripcion,
                    c.Fecha,
                    c.Maquina,
                    c.KgHora,
                    c.DesperdicioTotal,
                    c.VariablesCriticasJson,
                    Puntaje =
                        (maxKgHora > 0
                            ? (double)(c.KgHora / maxKgHora)
                            : 0)
                        -
                        (maxDesperdicio > 0
                            ? (double)(c.DesperdicioTotal / maxDesperdicio)
                            : 0)
                })
                .OrderByDescending(c => c.Puntaje)
                .ToList();

            if (fijado != null)
            {
                var kgHoraFijado = decimal.TryParse(
                    fijado.ProduccionKgHora, out var kgF) ? kgF : 0;

                var descripcionFijada =
                    fijado.Referencia != null &&
                    descripciones.TryGetValue(
                        fijado.Referencia, out var descripcion)
                        ? descripcion
                        : "";

                return (
                    StatusCodes.Status200OK,
                    new
                    {
                        totalEncontrados = conPuntaje.Count,
                        mejor = new
                        {
                            fijado.Id,
                            fijado.OrdenProduccion,
                            fijado.Referencia,
                            Descripcion = descripcionFijada,
                            fijado.Fecha,
                            fijado.Maquina,
                            KgHora = kgHoraFijado,
                            fijado.VariablesCriticasJson,
                            EsFijado = true
                        },
                        todos = conPuntaje
                    }
                );
            }

            return (
                StatusCodes.Status200OK,
                new
                {
                    totalEncontrados = conPuntaje.Count,
                    mejor = conPuntaje.First(),
                    todos = conPuntaje
                }
            );
        }


        // POST: registros/{id}/parametros-operario
        public async Task GuardarParametrosOperarioAsync(
            int id,
            GuardarParametrosOperarioDto dto)
        {
            var existente = await _context.RegistrosParametrosOperario
                .FirstOrDefaultAsync(p =>
                    p.RegistroFormatoCalidadId == id &&
                    p.OperarioNombre == dto.OperarioNombre);

            if (existente != null)
            {
                existente.VariablesCriticasJson = dto.VariablesCriticasJson;
                existente.MotivoCambio = dto.MotivoCambio;
                existente.FechaGuardado = DateTime.Now;
            }
            else
            {
                _context.RegistrosParametrosOperario.Add(
                    new RegistroParametrosOperario
                    {
                        RegistroFormatoCalidadId = id,
                        OperarioNombre = dto.OperarioNombre,
                        VariablesCriticasJson = dto.VariablesCriticasJson,
                        MotivoCambio = dto.MotivoCambio,
                        FechaGuardado = DateTime.Now
                    });
            }

            await _context.SaveChangesAsync();
        }

        // GET: registros/{id}/parametros-operario
        public async Task<List<RegistroParametrosOperario>>
            ObtenerParametrosOperarioAsync(int id)
        {
            return await _context.RegistrosParametrosOperario
                .Where(p => p.RegistroFormatoCalidadId == id)
                .OrderByDescending(p => p.FechaGuardado)
                .ToListAsync();
        }

        // POST: registros/{id}/rondas/{rondaId}/fotos
        public async Task<(
            List<object> Fotos,
            int StatusCode,
            string? Mensaje)>
            SubirFotosRondaAsync(
                int id,
                string rondaId,
                List<IFormFile> fotos)
        {
            var usuario = ObtenerUsuarioActual();

            if (string.IsNullOrWhiteSpace(rondaId) || rondaId.Length > 50)
            {
                return (
                    new List<object>(),
                    StatusCodes.Status400BadRequest,
                    "Identificador de hora no válido"
                );
            }

            if (fotos == null || fotos.Count == 0)
            {
                return (
                    new List<object>(),
                    StatusCodes.Status400BadRequest,
                    "No se recibieron fotos"
                );
            }

            var registro = await _context.RegistrosFormatoCalidad
                .FindAsync(id);

            if (registro == null)
            {
                return (
                    new List<object>(),
                    StatusCodes.Status404NotFound,
                    null
                );
            }

            var existentes = await ContarFotosRondaAsync(registro, rondaId);

            if (existentes + fotos.Count > MaxFotosRonda)
            {
                return (
                    new List<object>(),
                    StatusCodes.Status400BadRequest,
                    $"Cada hora admite máximo {MaxFotosRonda} fotos (ya tiene {existentes})"
                );
            }

            foreach (var foto in fotos)
            {
                var extension = Path.GetExtension(foto.FileName)
                    .ToLowerInvariant();

                if (!ExtensionesFotoRonda.Contains(extension) ||
                    !foto.ContentType.StartsWith(
                        "image/",
                        StringComparison.OrdinalIgnoreCase))
                {
                    return (
                        new List<object>(),
                        StatusCodes.Status400BadRequest,
                        $"El archivo '{foto.FileName}' no es una imagen válida (JPG, PNG o WEBP)"
                    );
                }

                if (foto.Length == 0 || foto.Length > MaxBytesFotoRonda)
                {
                    return (
                        new List<object>(),
                        StatusCodes.Status400BadRequest,
                        $"El archivo '{foto.FileName}' es demasiado grande (máximo 8 MB)"
                    );
                }
            }

            var carpeta = Path.Combine(
                _environment.WebRootPath
                    ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"),
                "formatos-calidad");

            Directory.CreateDirectory(carpeta);

            var nuevas = new List<FormatoCalidadFoto>();

            foreach (var foto in fotos)
            {
                var nombre = Guid.NewGuid() +
                    Path.GetExtension(foto.FileName).ToLowerInvariant();

                var ruta = Path.Combine(carpeta, nombre);

                using (var stream = new FileStream(ruta, FileMode.Create))
                {
                    await foto.CopyToAsync(stream);
                }

                var entidad = new FormatoCalidadFoto
                {
                    RegistroFormatoCalidadId = id,
                    RondaId = rondaId,
                    Archivo = nombre,
                    SubidaPor = usuario
                };

                _context.FormatosCalidadFotos.Add(entidad);
                nuevas.Add(entidad);
            }

            await _context.SaveChangesAsync();

            var resultado = nuevas.Select(f => (object)new
            {
                f.Id,
                f.RondaId,
                f.Archivo,
                f.FechaCreacion,
                f.SubidaPor
            }).ToList();

            return (resultado, StatusCodes.Status200OK, null);
        }

        // GET: registros/{id}/fotos
        public async Task<List<object>> ObtenerFotosRondasAsync(int id)
        {
            return await _context.FormatosCalidadFotos
                .Where(f => f.RegistroFormatoCalidadId == id)
                .OrderBy(f => f.Id)
                .Select(f => (object)new
                {
                    f.Id,
                    f.RondaId,
                    f.Archivo,
                    f.FechaCreacion,
                    f.SubidaPor
                })
                .ToListAsync();
        }

        // DELETE: registros/{id}/fotos/{fotoId}
        public async Task<(
            bool Exito,
            int StatusCode,
            string? Mensaje)>
            EliminarFotoRondaAsync(int id, int fotoId)
        {
            var usuario = ObtenerUsuarioActual();
            var rol = ObtenerRolActual();
            var usuarioId = ObtenerIdUsuarioActual();

            var puedeEditar = rol == "Admin"
                || await _context.UsuarioPermisos.AnyAsync(p =>
                    p.UsuarioId == usuarioId &&
                    p.Modulo == "calidad-formatos" &&
                    p.PuedeEditar);

            if (!puedeEditar)
            {
                return (
                    false,
                    StatusCodes.Status403Forbidden,
                    "No tiene permiso para eliminar fotos"
                );
            }

            var foto = await _context.FormatosCalidadFotos
                .FirstOrDefaultAsync(f =>
                    f.Id == fotoId &&
                    f.RegistroFormatoCalidadId == id);

            if (foto == null)
            {
                return (
                    false,
                    StatusCodes.Status404NotFound,
                    null
                );
            }

            _context.FormatosCalidadFotos.Remove(foto);
            await _context.SaveChangesAsync();

            var carpeta = Path.Combine(
                _environment.WebRootPath
                    ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"),
                "formatos-calidad");

            var ruta = Path.Combine(
                carpeta,
                Path.GetFileName(foto.Archivo));

            if (File.Exists(ruta))
            {
                try
                {
                    File.Delete(ruta);
                }
                catch
                {
                    // La eliminación de la foto en la base de datos
                    // no se revierte si falla la eliminación del archivo.
                }
            }

            await _auditoria.RegistrarAsync(
                usuario,
                rol,
                "Eliminar-Foto",
                "FormatosCalidad",
                $"Foto eliminada del registro {id}",
                id);

            return (true, StatusCodes.Status200OK, null);
        }

        // PUT: registros/{id}/fijar-mejor
        public async Task<RegistroFormatoCalidad?> FijarMejorRendimientoAsync(
            int id)
        {
            var registro = await _context.RegistrosFormatoCalidad
                .FindAsync(id);

            if (registro == null)
                return null;

            var otros = await _context.RegistrosFormatoCalidad
                .Where(r =>
                    r.Referencia == registro.Referencia &&
                    r.Maquina == registro.Maquina &&
                    r.Id != id)
                .ToListAsync();

            foreach (var otro in otros)
                otro.EsMejorFijado = false;

            registro.EsMejorFijado = true;

            await _context.SaveChangesAsync();

            return registro;
        }

        // PUT: registros/{id}/quitar-fijado
        public async Task<RegistroFormatoCalidad?> QuitarFijadoAsync(int id)
        {
            var registro = await _context.RegistrosFormatoCalidad
                .FindAsync(id);

            if (registro == null)
                return null;

            registro.EsMejorFijado = false;

            await _context.SaveChangesAsync();

            return registro;
        }

        // Cuenta las fotos de la tabla y las fotos heredadas del JSON.
        private async Task<int> ContarFotosRondaAsync(
            RegistroFormatoCalidad registro,
            string rondaId)
        {
            var enTabla = await _context.FormatosCalidadFotos
                .CountAsync(f =>
                    f.RegistroFormatoCalidadId == registro.Id &&
                    f.RondaId == rondaId);

            var enJson = 0;

            try
            {
                var raiz = JsonNode.Parse(registro.ResultadosJson ?? "{}");

                if (raiz?["rondas"] is JsonArray rondas)
                {
                    foreach (var ronda in rondas)
                    {
                        var id = ronda?["id"]?.GetValue<string>();

                        if (id == rondaId &&
                            ronda?["fotos"] is JsonArray fotosJson)
                        {
                            enJson = fotosJson.Count;
                        }
                    }
                }
            }
            catch
            {
                // Se ignora el JSON inválido, como en el comportamiento original.
            }

            return enTabla + enJson;
        }

        // Suma el desperdicio de las rondas cerradas por el operario.
        private static decimal ObtenerDesperdicioDesdeJson(
            string? resultadosJson)
        {
            decimal total = 0;

            try
            {
                using var datos = JsonDocument.Parse(resultadosJson ?? "{}");

                if (datos.RootElement.TryGetProperty("rondas", out var rondas) &&
                    rondas.ValueKind == JsonValueKind.Array)
                {
                    foreach (var ronda in rondas.EnumerateArray())
                    {
                        if (ronda.TryGetProperty(
                                "cierreDeOperario",
                                out var cierre) &&
                            cierre.ValueKind == JsonValueKind.True &&
                            ronda.TryGetProperty(
                                "kilosDesperdicio",
                                out var kdElemento) &&
                            kdElemento.ValueKind == JsonValueKind.String)
                        {
                            var texto = kdElemento.GetString();

                            if (decimal.TryParse(texto, out var desperdicio))
                                total += desperdicio;
                        }
                    }
                }
            }
            catch
            {
                // Se ignora el JSON inválido, como en el comportamiento original.
            }

            return total;
        }
    }
}
