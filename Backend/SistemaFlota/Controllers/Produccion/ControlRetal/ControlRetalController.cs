using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaFlota.Services.Produccion.ControlRetal;

namespace SistemaFlota.Controllers.Produccion.ControlRetal
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ControlRetalController : ControllerBase
    {
        private readonly IControlRetalService _controlRetalService;

        public ControlRetalController(IControlRetalService controlRetalService)
        {
            _controlRetalService = controlRetalService;
        }

        [HttpGet("orden/{numop:int}")]
        public async Task<IActionResult> ObtenerReferencia(int numop)
        {
            var resultado = await _controlRetalService
                .ObtenerReferenciaPorOrdenAsync(numop);

            if (resultado == null)
            {
                return NotFound(new
                {
                    mensaje = $"No se encontró la orden de producción {numop}."
                });
            }

            return Ok(resultado);
        }
    }
}
