using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace SistemaFlota.Controllers.VistasInventarios
{
    [ApiController]
    [Route("api/[controller]")]
    public class VistaInventariosController : ControllerBase
    {
        private readonly AppDbContext _context;

        public VistaInventariosController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("referencias")]
        public async Task<IActionResult> ObtenerReferencias()
        {
            var data = await _context.VInvReferencias
                .AsNoTracking()
                .ToListAsync();

            return Ok(data);
        }

        [HttpGet("ordenes-produccion")]
        public async Task<IActionResult> ObtenerOrdenesProduccion()
        {
            var data = await _context.VInvOrdenesProduccion
                .AsNoTracking()
                .ToListAsync();

            return Ok(data);
        }

        [HttpGet("registro-produccion")]
        public async Task<IActionResult> ObtenerRegistroProduccion()
        {
            var data = await _context.VInvRegistroProduccion
                .AsNoTracking()
                .ToListAsync();

            return Ok(data);
        }
    }
}