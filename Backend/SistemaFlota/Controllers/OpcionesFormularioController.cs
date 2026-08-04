using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using SistemaFlota.DTOs;
using SistemaFlota.Models;

namespace SistemaFlota.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class OpcionesFormularioController : ControllerBase
    {
        private readonly AppDbContext _context;

        public OpcionesFormularioController(AppDbContext context)
        {
            _context = context;
        }

        // GET api/OpcionesFormulario?categoria=Maquina&tipoFormatoId=1
        // Devuelve las opciones globales (TipoFormatoId=null) MÁS las específicas del tipo, si aplica
        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] string categoria, [FromQuery] int? tipoFormatoId = null)
        {
            var query = _context.OpcionesFormulario
                .Where(o => o.Categoria == categoria && o.Activo &&
                    (o.TipoFormatoId == null || o.TipoFormatoId == tipoFormatoId));

            var lista = await query
                .OrderBy(o => o.Orden)
                .Select(o => new { o.Id, o.Valor, o.TipoFormatoId })
                .ToListAsync();

            return Ok(lista);
        }

        // GET api/OpcionesFormulario/todas — para la pantalla de administración
        [HttpGet("todas")]
        public async Task<IActionResult> GetTodas()
        {
            var lista = await _context.OpcionesFormulario
                .Include(o => o.TipoFormato)
                .OrderBy(o => o.Categoria).ThenBy(o => o.Orden)
                .Select(o => new {
                    o.Id,
                    o.Categoria,
                    o.Valor,
                    o.Orden,
                    o.Activo,
                    o.TipoFormatoId,
                    TipoFormatoNombre = o.TipoFormato != null ? o.TipoFormato.Nombre : null
                })
                .ToListAsync();

            return Ok(lista);
        }

        // GET api/OpcionesFormulario/categorias — lista de categorías existentes
        [HttpGet("categorias")]
        public async Task<IActionResult> GetCategorias()
        {
            var categorias = await _context.OpcionesFormulario
                .Select(o => o.Categoria)
                .Distinct()
                .ToListAsync();
            return Ok(categorias);
        }

        // POST api/OpcionesFormulario
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] OpcionFormularioDto dto)
        {
            var opcion = new OpcionFormulario
            {
                Categoria = dto.Categoria,
                TipoFormatoId = dto.TipoFormatoId,
                Valor = dto.Valor,
                Orden = dto.Orden,
                Activo = true
            };
            _context.OpcionesFormulario.Add(opcion);
            await _context.SaveChangesAsync();
            return Ok(opcion);
        }

        // PUT api/OpcionesFormulario/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] OpcionFormularioDto dto)
        {
            var opcion = await _context.OpcionesFormulario.FindAsync(id);
            if (opcion == null) return NotFound();

            opcion.Categoria = dto.Categoria;
            opcion.TipoFormatoId = dto.TipoFormatoId;
            opcion.Valor = dto.Valor;
            opcion.Orden = dto.Orden;

            await _context.SaveChangesAsync();
            return Ok(opcion);
        }

        // PUT api/OpcionesFormulario/{id}/estado — activar/desactivar
        [HttpPut("{id}/estado")]
        public async Task<IActionResult> CambiarEstado(int id)
        {
            var opcion = await _context.OpcionesFormulario.FindAsync(id);
            if (opcion == null) return NotFound();
            opcion.Activo = !opcion.Activo;
            await _context.SaveChangesAsync();
            return Ok(opcion);
        }

        // DELETE api/OpcionesFormulario/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var opcion = await _context.OpcionesFormulario.FindAsync(id);
            if (opcion == null) return NotFound();
            _context.OpcionesFormulario.Remove(opcion);
            await _context.SaveChangesAsync();
            return Ok();
        }
    }
}
