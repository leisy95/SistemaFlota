using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using SistemaFlota.DTOs;

namespace SistemaFlota
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class FormatoFGC008Controller : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly AuditoriaService _auditoria;
        public FormatoFGC008Controller(AppDbContext context, AuditoriaService auditoria)
        {
            _context = context;
            _auditoria = auditoria;
        }
        private string GetUsuario() => User.FindFirst(ClaimTypes.Name)?.Value ?? "Desconocido";
        private string GetRol() => User.FindFirst(ClaimTypes.Role)?.Value ?? "Desconocido";

        [HttpGet]
        public async Task<IActionResult> Get(
            [FromQuery] DateTime? desde,
            [FromQuery] DateTime? hasta,
            [FromQuery] string? op)
        {
            var query = _context.FormatosFGC008.AsQueryable();
            if (desde.HasValue) query = query.Where(f => f.Fecha >= desde.Value);
            if (hasta.HasValue) query = query.Where(f => f.Fecha <= hasta.Value.AddDays(1));
            if (!string.IsNullOrWhiteSpace(op)) query = query.Where(f => f.OrdenProduccion.Contains(op));
            var lista = await query.OrderByDescending(f => f.Fecha).ToListAsync();
            return Ok(lista);
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromForm] FormatoFGC008Dto dto, IFormFile? foto)
        {
            string? nombreFoto = null;
            if (foto != null)
            {
                var carpeta = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/formatos");
                if (!Directory.Exists(carpeta)) Directory.CreateDirectory(carpeta);
                nombreFoto = Guid.NewGuid().ToString() + Path.GetExtension(foto.FileName);
                using var stream = new FileStream(Path.Combine(carpeta, nombreFoto), FileMode.Create);
                await foto.CopyToAsync(stream);
            }
            var registro = new FormatoFGC008
            {
                OrdenProduccion = dto.OrdenProduccion,
                Cliente = dto.Cliente,
                Referencia = dto.Referencia,
                EtiquetasSI = dto.EtiquetasSI,
                EmbalajeSI = dto.EmbalajeSI,
                DefectosSI = dto.DefectosSI,
                CantidadOP = dto.CantidadOP,
                CantidadReal = dto.CantidadReal,
                ListoBodega = dto.ListoBodega,
                Despachado = dto.Despachado,
                AccionesTomadas = dto.AccionesTomadas,
                FotoEvidencia = nombreFoto,
                FirmaDigital = dto.FirmaDigital,
                RevisadoPor = GetUsuario(),
                FechaRevision = DateTime.Now
            };
            _context.FormatosFGC008.Add(registro);
            await _context.SaveChangesAsync();
            await _auditoria.RegistrarAsync(GetUsuario(), GetRol(), "Crear", "FormatoFGC008", $"Registro OP: {dto.OrdenProduccion}", registro.Id);
            return Ok(registro);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromForm] FormatoFGC008Dto dto, IFormFile? foto)
        {
            Console.WriteLine($"🔍 DIAGNOSTICO - FirmaDigital recibida: {(string.IsNullOrEmpty(dto.FirmaDigital) ? "VACIA/NULL" : $"SI, largo={dto.FirmaDigital.Length}")}");
            var r = await _context.FormatosFGC008.FindAsync(id);
            if (r == null) return NotFound();
            r.OrdenProduccion = dto.OrdenProduccion;
            r.Cliente = dto.Cliente;
            r.Referencia = dto.Referencia;
            r.EtiquetasSI = dto.EtiquetasSI;
            r.EmbalajeSI = dto.EmbalajeSI;
            r.DefectosSI = dto.DefectosSI;
            r.CantidadOP = dto.CantidadOP;
            r.CantidadReal = dto.CantidadReal;
            r.ListoBodega = dto.ListoBodega;
            r.Despachado = dto.Despachado;
            r.AccionesTomadas = dto.AccionesTomadas;
            r.FirmaDigital = dto.FirmaDigital;
            if (foto != null)
            {
                var carpeta = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/formatos");
                if (!Directory.Exists(carpeta)) Directory.CreateDirectory(carpeta);
                var nombreFoto = Guid.NewGuid().ToString() + Path.GetExtension(foto.FileName);
                using var stream = new FileStream(Path.Combine(carpeta, nombreFoto), FileMode.Create);
                await foto.CopyToAsync(stream);
                r.FotoEvidencia = nombreFoto;
            }
            await _context.SaveChangesAsync();
            return Ok(r);
        }

        [HttpGet("op/{op}")]
        public async Task<IActionResult> GetPorOP(string op)
        {
            var registros = await _context.FormatosFGC008
                .Where(f => f.OrdenProduccion == op)
                .OrderByDescending(f => f.Fecha)
                .ToListAsync();
            if (!registros.Any()) return NotFound();
            return Ok(new
            {
                cantidadOP = registros.First().CantidadOP,
                totalEntradas = registros.Count,
                totalReal = registros.Sum(f => f.CantidadReal)
            });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var r = await _context.FormatosFGC008.FindAsync(id);
            if (r == null) return NotFound();
            _context.FormatosFGC008.Remove(r);
            await _context.SaveChangesAsync();
            return Ok();
        }
    }
}