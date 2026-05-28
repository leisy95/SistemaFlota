using Microsoft.AspNetCore.Mvc;

using Microsoft.EntityFrameworkCore;

using Microsoft.AspNetCore.Authorization;

namespace SistemaFlota
{
    [ApiController]

    [Route("api/[controller]")]

    [Authorize]

    public class ChecklistItemsController
        : ControllerBase
    {
        private readonly AppDbContext
            _context;

        public ChecklistItemsController(
            AppDbContext context
        )
        {
            _context = context;
        }

        // =====================================
        // GET POR TIPO VEHÍCULO
        // =====================================

        [HttpGet("{tipoVehiculoId}")]

        public async Task<IActionResult>
            Get(
                int tipoVehiculoId
            )
        {
            var lista =

                await _context
                .ChecklistItems

                .Where(c =>

                    c.TipoVehiculoId
                    == tipoVehiculoId

                    &&

                    c.Activo
                )

                .ToListAsync();

            return Ok(lista);
        }

        // =====================================
        // GET TODOS
        // =====================================

        [HttpGet]

        public async Task<IActionResult>
            GetTodos()
        {
            var lista =

                await _context
                .ChecklistItems
                .ToListAsync();

            return Ok(lista);
        }

        // =====================================
        // POST
        // =====================================

        [HttpPost]

        public async Task<IActionResult>
            Post(
                ChecklistItem item
            )
        {
            item.Activo =
                true;

            _context
                .ChecklistItems
                .Add(item);

            await _context
                .SaveChangesAsync();

            return Ok(item);
        }

        // =====================================
        // PUT
        // =====================================

        [HttpPut("{id}")]

        public async Task<IActionResult>
            Put(
                int id,

                ChecklistItem data
            )
        {
            var item =

                await _context
                .ChecklistItems
                .FindAsync(id);

            if (item == null)
            {
                return NotFound();
            }

            item.Descripcion =
                data.Descripcion;

            item.TipoVehiculoId =
                data.TipoVehiculoId;

            await _context
                .SaveChangesAsync();

            return Ok(item);
        }

        // =====================================
        // DELETE
        // =====================================

        [HttpDelete("{id}")]

        public async Task<IActionResult>
            Delete(int id)
        {
            var item =

                await _context
                .ChecklistItems
                .FindAsync(id);

            if (item == null)
            {
                return NotFound();
            }

            _context
                .ChecklistItems
                .Remove(item);

            await _context
                .SaveChangesAsync();

            return Ok();
        }
    }
}