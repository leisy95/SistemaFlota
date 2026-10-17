
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaFlota.Authorization;
using SistemaFlota.DTOs.Costos.DashboardEjecutivo;
using SistemaFlota.Services.Costos.DashboardEjecutivo;

namespace SistemaFlota.Controllers.Costos.DashboardEjecutivo
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DashboardEjecutivoController : ControllerBase
    {
        private readonly IDashboardEjecutivoService _service;

        public DashboardEjecutivoController(
            IDashboardEjecutivoService service)
        {
            _service = service;
        }

        // Consultar dashboard ejecutivo
        [HttpGet]
        [Permiso("dashboard-compras", "ver")]
        [ProducesResponseType(
            typeof(DashboardEjecutivoDto),
            StatusCodes.Status200OK)]
        public async Task<ActionResult<DashboardEjecutivoDto>> Obtener(
            [FromQuery] int pagina = 1,
            [FromQuery] int porPagina = 5,
            [FromQuery] DateTime? fechaDesde = null,
            [FromQuery] DateTime? fechaHasta = null,
            [FromQuery] string? proveedor = null,
            [FromQuery] string? estado = null,
            [FromQuery] string? categoria = null,
            [FromQuery] string? densidad = null)
        {
            var resultado = await _service.ObtenerDashboardAsync(
                pagina,
                porPagina,
                fechaDesde,
                fechaHasta,
                proveedor,
                estado,
                categoria,
                densidad
            );

            return Ok(resultado);
        }

        // Obtener opciones para los filtros
        [HttpGet("filtros")]
        [Permiso("dashboard-compras", "ver")]
        [ProducesResponseType(
            typeof(FiltrosDashboardEjecutivoDto),
            StatusCodes.Status200OK)]
        public async Task<ActionResult<FiltrosDashboardEjecutivoDto>>
            ObtenerFiltros()
        {
            var resultado = await _service.ObtenerFiltrosAsync();

            return Ok(resultado);
        }
    }
}
