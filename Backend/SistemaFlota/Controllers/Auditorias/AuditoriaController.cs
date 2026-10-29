
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaFlota.DTOs.Auditorias;
using SistemaFlota.Services.Auditorias;

namespace SistemaFlota
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AuditoriaController : ControllerBase
    {
        private readonly IAuditoriaService _service;

        public AuditoriaController(IAuditoriaService service)
        {
            _service = service;
        }

        // GET: api/Auditoria
        [HttpGet]
        public async Task<IActionResult> Get(
            [FromQuery] string? usuario,
            [FromQuery] string? modulo,
            [FromQuery] string? accion,
            [FromQuery] string? resultado,
            [FromQuery] DateTime? fechaDesde,
            [FromQuery] DateTime? fechaHasta,
            [FromQuery] int pagina = 1,
            [FromQuery] int porPagina = 50)
        {
            if (pagina < 1)
            {
                return BadRequest(new
                {
                    mensaje = "La página debe ser mayor o igual a 1."
                });
            }

            if (porPagina < 1 || porPagina > 100)
            {
                return BadRequest(new
                {
                    mensaje = "Por página debe estar entre 1 y 100."
                });
            }

            if (fechaDesde.HasValue &&
                fechaHasta.HasValue &&
                fechaDesde.Value.Date > fechaHasta.Value.Date)
            {
                return BadRequest(new
                {
                    mensaje = "La fecha inicial no puede ser posterior a la fecha final."
                });
            }

            var filtro = new AuditoriaFiltroDto
            {
                Usuario = usuario,
                Modulo = modulo,
                Accion = accion,
                Resultado = resultado,
                Desde = fechaDesde,
                Hasta = fechaHasta
            };

            var resultadoPaginado = await _service.ObtenerPaginadoAsync(
                filtro,
                pagina,
                porPagina);

            // Conservamos los nombres que entrega el controlador anterior.
            return Ok(new
            {
                total = resultadoPaginado.TotalRegistros,
                pagina = resultadoPaginado.Pagina,
                porPagina = resultadoPaginado.PorPagina,
                totalPaginas = resultadoPaginado.TotalPaginas,
                datos = resultadoPaginado.Registros
            });
        }

        // GET: api/Auditoria/estadisticas
        [HttpGet("estadisticas")]
        public async Task<IActionResult> GetEstadisticas()
        {
            var estadisticas = await _service.ObtenerEstadisticasAsync();

            return Ok(estadisticas);
        }

        // DELETE: api/Auditoria/limpiar?dias=90
        [HttpDelete("limpiar")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Limpiar(
            [FromQuery] int dias = 90)
        {
            if (dias < 1)
            {
                return BadRequest(new
                {
                    mensaje = "Los días deben ser mayores que cero."
                });
            }

            var eliminados = await _service.LimpiarAsync(dias);

            return Ok(new { eliminados });
        }
    }
}
