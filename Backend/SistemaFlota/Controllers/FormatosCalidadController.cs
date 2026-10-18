using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaFlota.DTOs;
using SistemaFlota.Models;
using SistemaFlota.Models.Calidad;
using System.Security.Claims;

namespace SistemaFlota
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class FormatosCalidadController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly AuditoriaService _auditoria;
        private readonly IMensajeriaService _mensajeria;
        private readonly EmpresaOrdenesService _empresaOrdenes;

        public FormatosCalidadController(AppDbContext context, AuditoriaService auditoria, IMensajeriaService twilio, EmpresaOrdenesService empresaOrdenes)
        {
            _context = context;
            _auditoria = auditoria;
            _mensajeria = twilio;
            _empresaOrdenes = empresaOrdenes;
        }

        private string GetUsuario() => User.FindFirst(ClaimTypes.Name)?.Value ?? "Desconocido";
        private string GetRol() => User.FindFirst(ClaimTypes.Role)?.Value ?? "Desconocido";

        // GET api/FormatosCalidad/tipos — lista los 4 tipos disponibles
        [HttpGet("tipos")]
        public async Task<IActionResult> GetTipos()
        {
            var tipos = await _context.TiposFormatoCalidad
                .OrderBy(t => t.Codigo)
                .Select(t => new { t.Id, t.Codigo, t.Nombre, t.TieneVariablesCriticas })
                .ToListAsync();
            return Ok(tipos);
        }

        // GET api/FormatosCalidad/tipos/{codigo}/caracteristicas — ej: F-GC-004
        [HttpGet("tipos/{codigo}/caracteristicas")]
        public async Task<IActionResult> GetCaracteristicas(string codigo)
        {
            var tipo = await _context.TiposFormatoCalidad
                .Include(t => t.Caracteristicas)
                .FirstOrDefaultAsync(t => t.Codigo == codigo);
            if (tipo == null) return NotFound();

            return Ok(new
            {
                tipo.Id,
                tipo.Codigo,
                tipo.Nombre,
                tipo.TieneVariablesCriticas,
                Caracteristicas = tipo.Caracteristicas.OrderBy(c => c.Orden)
                    .Select(c => new { c.Id, c.Orden, c.Descripcion })
            });
        }

        // GET api/FormatosCalidad/registros?codigo=F-GC-004
        [HttpGet("registros")]
        public async Task<IActionResult> GetRegistros(
            [FromQuery] string codigo,
            [FromQuery] DateTime? desde,
            [FromQuery] DateTime? hasta,
            [FromQuery] string? op)
        {
            var tipo = await _context.TiposFormatoCalidad.FirstOrDefaultAsync(t => t.Codigo == codigo);
            if (tipo == null) return NotFound(new { mensaje = "Tipo de formato no encontrado" });

            var query = _context.RegistrosFormatoCalidad.Where(r => r.TipoFormatoId == tipo.Id);
            if (desde.HasValue) query = query.Where(r => r.Fecha >= desde.Value);
            if (hasta.HasValue) query = query.Where(r => r.Fecha <= hasta.Value.AddDays(1));
            if (!string.IsNullOrWhiteSpace(op)) query = query.Where(r => r.OrdenProduccion.Contains(op));

            var lista = await query.OrderByDescending(r => r.Fecha).ToListAsync();
            return Ok(lista);
        }

        // POST api/FormatosCalidad/registros — el operario crea el registro (sin liberar aún)
        [HttpPost("registros")]
        public async Task<IActionResult> Post([FromBody] RegistroFormatoCalidadDto dto)
        {
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

            // Fecha real de producción (órdenes físicas que se ingresan después)
            DateTime? fechaManual = null;
            if (dto.FechaRealProduccion.HasValue)
            {
                var cuentasMaquina = new[] { "Extrusion", "Precorte", "Impresion", "Sellado" };
                var esCuentaMaquina = cuentasMaquina.Contains(GetUsuario(), StringComparer.OrdinalIgnoreCase);

                int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var usuarioId);
                var puedeEditar = GetRol() == "Admin"
                    || await _context.UsuarioPermisos.AnyAsync(p =>
                        p.UsuarioId == usuarioId && p.Modulo == "calidad-formatos" && p.PuedeEditar);

                if (esCuentaMaquina || !puedeEditar)
                    return StatusCode(403, new { mensaje = "No tiene permiso para fijar la fecha real de producción" });

                var fechaReal = dto.FechaRealProduccion.Value.Date;
                if (fechaReal > DateTime.Now.Date)
                    return BadRequest(new { mensaje = "La fecha real de producción no puede ser futura" });

                registro.Fecha = fechaReal;
                fechaManual = fechaReal;
            }

            _context.RegistrosFormatoCalidad.Add(registro);
            await _context.SaveChangesAsync();

            if (fechaManual.HasValue)
                await _auditoria.RegistrarAsync(GetUsuario(), GetRol(), "Fecha-Manual", "FormatosCalidad",
                    $"OP {registro.OrdenProduccion}: fecha real de producción {fechaManual:dd/MM/yyyy}, registrada el {DateTime.Now:dd/MM/yyyy HH:mm}",
                    registro.Id);

            await _auditoria.RegistrarAsync(GetUsuario(), GetRol(), "Crear", "FormatosCalidad",
                $"Registro OP: {dto.OrdenProduccion} - Tipo: {dto.TipoFormatoId}", registro.Id);

            // -- Notificar a los contactos que reciben avisos de liberación --
            var tipo = await _context.TiposFormatoCalidad.FindAsync(dto.TipoFormatoId);
            var contactos = await _context.ContactosNotificacion
                .Where(c => c.Activo && c.RecibeLiberaciones)
                .Select(c => c.NumeroWhatsApp)
                .ToListAsync();

            if (contactos.Any())
            {
                var mensaje = $"?? *PENDIENTE DE LIBERACIÓN*\n\n" +
                              $"Formato: {tipo?.Nombre} ({tipo?.Codigo})\n" +
                              $"Orden: {dto.OrdenProduccion}\n" +
                              $"Cliente: {dto.Cliente ?? "-"}\n" +
                              $"Registrado por: {GetUsuario()}\n\n" +
                              $"Por favor revisa y libera el producto en el sistema.";
                await _mensajeria.EnviarAMultiplesAsync(contactos, mensaje, "Liberaciones");
            }

            return Ok(registro);
        }

        // PUT api/FormatosCalidad/registros/{id} — edición general (antes de liberar)
        [HttpPut("registros/{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] RegistroFormatoCalidadDto dto)
        {
            var r = await _context.RegistrosFormatoCalidad.FindAsync(id);
            if (r == null) return NotFound();

            r.OrdenProduccion = dto.OrdenProduccion;
            r.Cliente = dto.Cliente;
            r.Referencia = dto.Referencia;
            r.Operarios = dto.Operarios;
            r.Hora = dto.Hora;
            r.Maquina = dto.Maquina;
            r.VariablesCriticasJson = dto.VariablesCriticasJson;
            r.ResultadosJson = dto.ResultadosJson;

            await _context.SaveChangesAsync();
            return Ok(r);
        }

        // PUT api/FormatosCalidad/registros/{id}/liberar — completa la liberación
        [HttpPut("registros/{id}/liberar")]
        public async Task<IActionResult> Liberar(int id, [FromBody] LiberarFormatoDto dto)
        {
            var r = await _context.RegistrosFormatoCalidad.FindAsync(id);
            if (r == null) return NotFound();

            r.PuedeLiberarse = dto.PuedeLiberarse;
            r.ExplicacionNoLiberado = dto.ExplicacionNoLiberado;
            r.FirmaDigital = dto.FirmaDigital;
            r.CargoFirma = dto.CargoFirma;
            r.ProduccionKgHora = dto.ProduccionKgHora;
            r.DesperdicioTotalKg = dto.DesperdicioTotalKg;
            r.RevisadoPor = GetUsuario();
            r.FechaRevision = DateTime.Now;
            r.Estado = "Liberado";

            await _context.SaveChangesAsync();

            await _auditoria.RegistrarAsync(GetUsuario(), GetRol(), "Liberar", "FormatosCalidad",
                $"Liberación OP: {r.OrdenProduccion} - Resultado: {(dto.PuedeLiberarse == true ? "SI" : "NO")}", r.Id);

            return Ok(r);
        }

        // GET api/FormatosCalidad/registros/op/{op}?tipoFormatoId=1
        [HttpGet("registros/op/{op}")]
        public async Task<IActionResult> GetPorOP(string op, [FromQuery] int tipoFormatoId)
        {
            var registro = await _context.RegistrosFormatoCalidad
                .Where(r => r.OrdenProduccion == op && r.TipoFormatoId == tipoFormatoId)
                .OrderByDescending(r => r.Fecha)
                .FirstOrDefaultAsync();
            if (registro == null) return NotFound();
            return Ok(registro);
        }

        // GET api/FormatosCalidad/mezcla/{op} — mezcla de la orden (solo lectura, viene de Kasoft)
        [HttpGet("mezcla/{op}")]
        public async Task<IActionResult> GetMezcla(string op, [FromServices] MezclaOrdenService mezcla)
        {
            try
            {
                return Ok(await mezcla.ObtenerAsync(op));
            }
            catch (Exception ex)
            {
                // Si Kasoft no responde, el formato sigue funcionando sin la mezcla
                Console.WriteLine($"Error consultando la mezcla de la OP {op}: {ex.Message}");
                return Ok(new List<MezclaOrdenItem>());
            }
        }

        // DELETE api/FormatosCalidad/registros/{id}
        [HttpDelete("registros/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var r = await _context.RegistrosFormatoCalidad.FindAsync(id);
            if (r == null) return NotFound();
            _context.RegistrosFormatoCalidad.Remove(r);
            await _context.SaveChangesAsync();
            return Ok();
        }

        // GET api/FormatosCalidad/desperdicio-orden?op=22310&tipoFormatoId=1
        [HttpGet("desperdicio-orden")]
        public async Task<IActionResult> DesperdicioPorOrden([FromQuery] string op, [FromQuery] int tipoFormatoId)
        {
            if (string.IsNullOrWhiteSpace(op))
                return BadRequest(new { mensaje = "Debe indicar el número de orden" });

            var registros = await _context.RegistrosFormatoCalidad
                .Where(r => r.OrdenProduccion == op && r.TipoFormatoId == tipoFormatoId)
                .ToListAsync();

            if (!registros.Any())
                return Ok(new { ordenProduccion = op, totalDesperdicio = 0m, totalRegistros = 0 });

            decimal totalDesperdicio = 0;

            foreach (var r in registros)
            {
                try
                {
                    var datos = System.Text.Json.JsonDocument.Parse(r.ResultadosJson ?? "{}");
                    if (datos.RootElement.TryGetProperty("rondas", out var rondas))
                    {
                        foreach (var ronda in rondas.EnumerateArray())
                        {
                            if (ronda.TryGetProperty("cierreDeOperario", out var cierre) && cierre.GetBoolean() &&
                                ronda.TryGetProperty("kilosDesperdicio", out var kdEl))
                            {
                                var kdTexto = kdEl.GetString();
                                if (decimal.TryParse(kdTexto, out var kd)) totalDesperdicio += kd;
                            }
                        }
                    }
                }
                catch { }
            }

            return Ok(new
            {
                ordenProduccion = op,
                totalDesperdicio,
                totalRegistros = registros.Count
            });
        }

        // GET api/FormatosCalidad/mejor-rendimiento?texto=XXX&maquina=YYY
        [HttpGet("mejor-rendimiento")]
        public async Task<IActionResult> MejorRendimiento([FromQuery] string texto, [FromQuery] string? maquina = null)
        {
            if (string.IsNullOrWhiteSpace(texto))
                return BadRequest(new { mensaje = "Debe indicar un texto para buscar" });

            var tipoExtrusion = await _context.TiposFormatoCalidad
                .FirstOrDefaultAsync(t => t.Codigo == "F-GC-004");
            if (tipoExtrusion == null) return NotFound(new { mensaje = "Tipo Extrusión no configurado" });

            var referenciasPorDescripcion = await _empresaOrdenes.BuscarReferenciasPorDescripcion(texto);

            var query = _context.RegistrosFormatoCalidad
                .Where(r => r.TipoFormatoId == tipoExtrusion.Id &&
                            r.ProduccionKgHora != null &&
                            (
                                (r.OrdenProduccion != null && r.OrdenProduccion.Contains(texto)) ||
                                (r.Referencia != null && r.Referencia.Contains(texto)) ||
                                (r.Cliente != null && r.Cliente.Contains(texto)) ||
                                (r.Referencia != null && referenciasPorDescripcion.Contains(r.Referencia))
                            ));

            if (!string.IsNullOrWhiteSpace(maquina))
                query = query.Where(r => r.Maquina == maquina);

            var registros = await query.ToListAsync();

            if (!registros.Any())
                return Ok(new { mensaje = "No hay registros históricos para esta búsqueda", resultados = new List<object>() });

            var fijado = registros.FirstOrDefault(r => r.EsMejorFijado);

            var calculados = registros.Select(r =>
            {
                var kgHora = decimal.TryParse(r.ProduccionKgHora, out var kg) ? kg : 0;

                decimal desperdicioTotal = 0;
                try
                {
                    var datos = System.Text.Json.JsonDocument.Parse(r.ResultadosJson ?? "{}");
                    if (datos.RootElement.TryGetProperty("rondas", out var rondas))
                    {
                        foreach (var ronda in rondas.EnumerateArray())
                        {
                            if (ronda.TryGetProperty("cierreDeOperario", out var cierre) && cierre.GetBoolean() &&
                                ronda.TryGetProperty("kilosDesperdicio", out var kdEl))
                            {
                                var kdTexto = kdEl.GetString();
                                if (decimal.TryParse(kdTexto, out var kd)) desperdicioTotal += kd;
                            }
                        }
                    }
                }
                catch { }

                return new
                {
                    r.Id,
                    r.OrdenProduccion,
                    r.Referencia,
                    r.Fecha,
                    r.Maquina,
                    KgHora = kgHora,
                    DesperdicioTotal = desperdicioTotal,
                    r.VariablesCriticasJson
                };
            }).ToList();

            var maxKgHora = calculados.Max(c => c.KgHora);
            var maxDesperdicio = calculados.Max(c => c.DesperdicioTotal);

            var conPuntaje = calculados.Select(c => new
            {
                c.Id,
                c.OrdenProduccion,
                c.Referencia,
                c.Fecha,
                c.Maquina,
                c.KgHora,
                c.DesperdicioTotal,
                c.VariablesCriticasJson,
                Puntaje = (maxKgHora > 0 ? (double)(c.KgHora / maxKgHora) : 0)
                        - (maxDesperdicio > 0 ? (double)(c.DesperdicioTotal / maxDesperdicio) : 0)
            })
            .OrderByDescending(c => c.Puntaje)
            .ToList();

            if (fijado != null)
            {
                var kgHoraFijado = decimal.TryParse(fijado.ProduccionKgHora, out var kgF) ? kgF : 0;
                return Ok(new
                {
                    totalEncontrados = conPuntaje.Count,
                    mejor = new
                    {
                        fijado.Id,
                        fijado.OrdenProduccion,
                        fijado.Referencia,
                        fijado.Fecha,
                        fijado.Maquina,
                        KgHora = kgHoraFijado,
                        fijado.VariablesCriticasJson,
                        EsFijado = true
                    },
                    todos = conPuntaje
                });
            }

            return Ok(new
            {
                totalEncontrados = conPuntaje.Count,
                mejor = conPuntaje.First(),
                todos = conPuntaje
            });
        }

        // POST api/FormatosCalidad/registros/{id}/parametros-operario
        [HttpPost("registros/{id}/parametros-operario")]
        public async Task<IActionResult> GuardarParametrosOperario(int id, [FromBody] GuardarParametrosOperarioDto dto)
        {
            var existente = await _context.RegistrosParametrosOperario
                .FirstOrDefaultAsync(p => p.RegistroFormatoCalidadId == id && p.OperarioNombre == dto.OperarioNombre);

            if (existente != null)
            {
                existente.VariablesCriticasJson = dto.VariablesCriticasJson;
                existente.MotivoCambio = dto.MotivoCambio;
                existente.FechaGuardado = DateTime.Now;
            }
            else
            {
                _context.RegistrosParametrosOperario.Add(new RegistroParametrosOperario
                {
                    RegistroFormatoCalidadId = id,
                    OperarioNombre = dto.OperarioNombre,
                    VariablesCriticasJson = dto.VariablesCriticasJson,
                    MotivoCambio = dto.MotivoCambio,
                    FechaGuardado = DateTime.Now
                });
            }

            await _context.SaveChangesAsync();
            return Ok();
        }

        // GET api/FormatosCalidad/registros/{id}/parametros-operario
        [HttpGet("registros/{id}/parametros-operario")]
        public async Task<IActionResult> GetParametrosOperario(int id)
        {
            var lista = await _context.RegistrosParametrosOperario
                .Where(p => p.RegistroFormatoCalidadId == id)
                .OrderByDescending(p => p.FechaGuardado)
                .ToListAsync();

            return Ok(lista);
        }

        private const int MaxFotosRonda = 5;
        private const long MaxBytesFotoRonda = 8 * 1024 * 1024;
        private static readonly string[] ExtensionesFotoRonda = { ".jpg", ".jpeg", ".png", ".webp" };

        // Cuántas fotos ya tiene una hora: las de la tabla más las que venían guardadas en el JSON (registros anteriores)
        private async Task<int> ContarFotosRondaAsync(RegistroFormatoCalidad registro, string rondaId)
        {
            var enTabla = await _context.FormatosCalidadFotos
                .CountAsync(f => f.RegistroFormatoCalidadId == registro.Id && f.RondaId == rondaId);

            var enJson = 0;
            try
            {
                var raiz = System.Text.Json.Nodes.JsonNode.Parse(registro.ResultadosJson ?? "{}");
                if (raiz?["rondas"] is System.Text.Json.Nodes.JsonArray rondas)
                {
                    foreach (var ronda in rondas)
                    {
                        // Si la hora todavía no tiene identificador, se compara por su posición (formato "pos-N")
                        var id = ronda?["id"]?.GetValue<string>();
                        if (id == rondaId && ronda?["fotos"] is System.Text.Json.Nodes.JsonArray fotosJson)
                            enJson = fotosJson.Count;
                    }
                }
            }
            catch { }

            return enTabla + enJson;
        }

        // POST api/FormatosCalidad/registros/{id}/rondas/{rondaId}/fotos
        [HttpPost("registros/{id}/rondas/{rondaId}/fotos")]
        public async Task<IActionResult> SubirFotosRonda(int id, string rondaId, [FromForm] List<IFormFile> fotos)
        {
            if (string.IsNullOrWhiteSpace(rondaId) || rondaId.Length > 50)
                return BadRequest(new { mensaje = "Identificador de hora no válido" });

            if (fotos == null || fotos.Count == 0)
                return BadRequest(new { mensaje = "No se recibieron fotos" });

            var registro = await _context.RegistrosFormatoCalidad.FindAsync(id);
            if (registro == null) return NotFound();

            var existentes = await ContarFotosRondaAsync(registro, rondaId);
            if (existentes + fotos.Count > MaxFotosRonda)
                return BadRequest(new { mensaje = $"Cada hora admite máximo {MaxFotosRonda} fotos (ya tiene {existentes})" });

            // Se valida todo antes de guardar nada, para no dejar la subida a medias
            foreach (var f in fotos)
            {
                var ext = Path.GetExtension(f.FileName).ToLowerInvariant();
                if (!ExtensionesFotoRonda.Contains(ext) || !f.ContentType.StartsWith("image/"))
                    return BadRequest(new { mensaje = $"El archivo '{f.FileName}' no es una imagen válida (JPG, PNG o WEBP)" });

                if (f.Length == 0 || f.Length > MaxBytesFotoRonda)
                    return BadRequest(new { mensaje = $"El archivo '{f.FileName}' es demasiado grande (máximo 8 MB)" });
            }

            var carpeta = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/formatos-calidad");
            if (!Directory.Exists(carpeta)) Directory.CreateDirectory(carpeta);

            var usuario = GetUsuario();
            var nuevas = new List<FormatoCalidadFoto>();

            foreach (var foto in fotos)
            {
                var nombre = Guid.NewGuid() + Path.GetExtension(foto.FileName).ToLowerInvariant();
                using (var stream = new FileStream(Path.Combine(carpeta, nombre), FileMode.Create))
                    await foto.CopyToAsync(stream);

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

            return Ok(nuevas.Select(f => new { f.Id, f.RondaId, f.Archivo, f.FechaCreacion, f.SubidaPor }));
        }

        // GET api/FormatosCalidad/registros/{id}/fotos — todas las fotos de las horas de un registro
        [HttpGet("registros/{id}/fotos")]
        public async Task<IActionResult> GetFotosRondas(int id)
        {
            var fotos = await _context.FormatosCalidadFotos
                .Where(f => f.RegistroFormatoCalidadId == id)
                .OrderBy(f => f.Id)
                .Select(f => new { f.Id, f.RondaId, f.Archivo, f.FechaCreacion, f.SubidaPor })
                .ToListAsync();

            return Ok(fotos);
        }

        // DELETE api/FormatosCalidad/registros/{id}/fotos/{fotoId}
        [HttpDelete("registros/{id}/fotos/{fotoId}")]
        public async Task<IActionResult> EliminarFotoRonda(int id, int fotoId)
        {
            // Solo quien puede editar este módulo puede borrar fotos ya guardadas
            int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var usuarioId);
            var puedeEditar = GetRol() == "Admin"
                || await _context.UsuarioPermisos.AnyAsync(p =>
                    p.UsuarioId == usuarioId && p.Modulo == "calidad-formatos" && p.PuedeEditar);

            if (!puedeEditar)
                return StatusCode(403, new { mensaje = "No tiene permiso para eliminar fotos" });

            var foto = await _context.FormatosCalidadFotos
                .FirstOrDefaultAsync(f => f.Id == fotoId && f.RegistroFormatoCalidadId == id);
            if (foto == null) return NotFound();

            _context.FormatosCalidadFotos.Remove(foto);
            await _context.SaveChangesAsync();

            // El archivo se borra después de la base; si falla, queda suelto pero no se ve en pantalla
            var ruta = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/formatos-calidad", Path.GetFileName(foto.Archivo));
            if (System.IO.File.Exists(ruta))
            {
                try { System.IO.File.Delete(ruta); } catch { }
            }

            await _auditoria.RegistrarAsync(GetUsuario(), GetRol(), "Eliminar-Foto", "FormatosCalidad",
                $"Foto eliminada del registro {id}", id);

            return Ok();
        }

        [HttpPut("{id}/fijar-mejor")]
        public async Task<IActionResult> FijarMejorRendimiento(int id)
        {
            var registro = await _context.RegistrosFormatoCalidad.FindAsync(id);
            if (registro == null) return NotFound();

            // Desfija cualquier otro registro con la misma Referencia + Máquina
            var otros = await _context.RegistrosFormatoCalidad
                .Where(r => r.Referencia == registro.Referencia && r.Maquina == registro.Maquina && r.Id != id)
                .ToListAsync();

            foreach (var otro in otros)
                otro.EsMejorFijado = false;

            registro.EsMejorFijado = true;
            await _context.SaveChangesAsync();

            return Ok(registro);
        }

        [HttpPut("{id}/quitar-fijado")]
        public async Task<IActionResult> QuitarFijado(int id)
        {
            var registro = await _context.RegistrosFormatoCalidad.FindAsync(id);
            if (registro == null) return NotFound();

            registro.EsMejorFijado = false;
            await _context.SaveChangesAsync();

            return Ok(registro);
        }
    }
}