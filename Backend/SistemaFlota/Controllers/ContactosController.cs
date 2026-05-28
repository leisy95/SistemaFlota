using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace SistemaFlota
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ContactosController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ContactosController(AppDbContext context)
        {
            _context = context;
        }

        // =====================================
        // GET TODOS
        // =====================================

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var lista = await _context.ContactosNotificacion
                .OrderBy(c => c.Area)
                .ToListAsync();

            return Ok(lista);
        }

        // =====================================
        // POST — CREAR
        // =====================================

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] ContactoNotificacion contacto)
        {
            _context.ContactosNotificacion.Add(contacto);
            await _context.SaveChangesAsync();
            return Ok(contacto);
        }

        // =====================================
        // PUT — EDITAR
        // =====================================

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] ContactoNotificacion dto)
        {
            var contacto = await _context.ContactosNotificacion.FindAsync(id);

            if (contacto == null)
                return NotFound();

            contacto.Nombre = dto.Nombre;
            contacto.Area = dto.Area;
            contacto.NumeroWhatsApp = dto.NumeroWhatsApp;
            contacto.Activo = dto.Activo;
            contacto.RecibeIncidentes = dto.RecibeIncidentes;

            await _context.SaveChangesAsync();
            return Ok(contacto);
        }

        // =====================================
        // DELETE
        // =====================================

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var contacto = await _context.ContactosNotificacion.FindAsync(id);

            if (contacto == null)
                return NotFound();

            _context.ContactosNotificacion.Remove(contacto);
            await _context.SaveChangesAsync();
            return Ok();
        }

        // =====================================
        // CAMBIAR ESTADO
        // =====================================

        [HttpPut("{id}/estado")]
        public async Task<IActionResult> CambiarEstado(int id)
        {
            var contacto = await _context.ContactosNotificacion.FindAsync(id);

            if (contacto == null)
                return NotFound();

            contacto.Activo = !contacto.Activo;
            await _context.SaveChangesAsync();
            return Ok(contacto);
        }
    }
}