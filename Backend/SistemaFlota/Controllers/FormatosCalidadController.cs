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

        public FormatosCalidadController(AppDbContext context, AuditoriaService auditoria)
        {
            _context = context;
            _auditoria = auditoria;
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

        // POST api/FormatosCalidad/registros
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
                PuedeLiberarse = dto.PuedeLiberarse,
                ExplicacionNoLiberado = dto.ExplicacionNoLiberado,
                FirmaDigital = dto.FirmaDigital,
                CargoFirma = dto.CargoFirma,
                RevisadoPor = GetUsuario(),
                FechaRevision = DateTime.Now
            };

            _context.RegistrosFormatoCalidad.Add(registro);
            await _context.SaveChangesAsync();

            await _auditoria.RegistrarAsync(GetUsuario(), GetRol(), "Crear", "FormatosCalidad",
                $"Registro OP: {dto.OrdenProduccion} - Tipo: {dto.TipoFormatoId}", registro.Id);

            return Ok(registro);
        }

        // PUT api/FormatosCalidad/registros/{id}
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
            r.PuedeLiberarse = dto.PuedeLiberarse;
            r.ExplicacionNoLiberado = dto.ExplicacionNoLiberado;
            r.FirmaDigital = dto.FirmaDigital;
            r.CargoFirma = dto.CargoFirma;

            await _context.SaveChangesAsync();
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
    }
}