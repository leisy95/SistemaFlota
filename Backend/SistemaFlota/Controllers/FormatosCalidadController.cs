using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaFlota.DTOs;
using SistemaFlota.Models;
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

        public FormatosCalidadController(AppDbContext context, AuditoriaService auditoria, IMensajeriaService twilio)
        {
            _context = context;
            _auditoria = auditoria;
            _mensajeria = twilio;
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
                Estado = "PendienteLiberacion",
                RevisadoPor = GetUsuario(),
                FechaRevision = DateTime.Now
            };

            _context.RegistrosFormatoCalidad.Add(registro);
            await _context.SaveChangesAsync();

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
            var registros = await _context.RegistrosFormatoCalidad
                .Where(r => r.OrdenProduccion == op && r.TipoFormatoId == tipoFormatoId)
                .OrderByDescending(r => r.Fecha)
                .ToListAsync();
            if (!registros.Any()) return NotFound();
            return Ok(new { totalEntradas = registros.Count });
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


        // GET api/FormatosCalidad/mejor-rendimiento?referencia=XXX
        [HttpGet("mejor-rendimiento")]
        public async Task<IActionResult> MejorRendimiento([FromQuery] string referencia, [FromQuery] string? maquina = null)
        {
            if (string.IsNullOrWhiteSpace(referencia))
                return BadRequest(new { mensaje = "Debe indicar una referencia para buscar" });

            var tipoExtrusion = await _context.TiposFormatoCalidad
                .FirstOrDefaultAsync(t => t.Codigo == "F-GC-004");
            if (tipoExtrusion == null) return NotFound(new { mensaje = "Tipo Extrusión no configurado" });

            var query = _context.RegistrosFormatoCalidad
                .Where(r => r.TipoFormatoId == tipoExtrusion.Id &&
                            r.Referencia != null && r.Referencia.Contains(referencia) &&
                            r.ProduccionKgHora != null);

            if (!string.IsNullOrWhiteSpace(maquina))
                query = query.Where(r => r.Maquina == maquina);

            var registros = await query.ToListAsync();

            if (!registros.Any())
                return Ok(new { mensaje = "No hay registros históricos para esta referencia", resultados = new List<object>() });

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

            return Ok(new
            {
                totalEncontrados = conPuntaje.Count,
                mejor = conPuntaje.First(),
                todos = conPuntaje
            });
        }
    }
}


