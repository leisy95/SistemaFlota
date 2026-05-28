using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace SistemaFlota
{
    [ApiController]

    [Route("api/[controller]")]

    public class TiposVehiculoController
        : ControllerBase
    {
        private readonly AppDbContext
            _context;

        public TiposVehiculoController(
            AppDbContext context
        )
        {
            _context = context;
        }

        // GET
        [HttpGet]

        public async Task<IActionResult>
            Get()
        {
            var lista =

                await _context
                .TiposVehiculo
                .ToListAsync();

            return Ok(lista);
        }
    }
}