
using Microsoft.AspNetCore.Mvc;
using SistemaFlota.Services.InformacionKasoft;

namespace SistemaFlota.Controllers.InformacionKasoft
{
    [ApiController]
    [Route("api/[controller]")]
    public class InformacionKasoftController : ControllerBase
    {
        private readonly IInformacionKasoftService _service;

        public InformacionKasoftController(
            IInformacionKasoftService service)
        {
            _service = service;
        }

        [HttpGet("stocksxreferencia")]
        public async Task<IActionResult> ObtenerStocksPorReferencia()
        {
            return Ok(await _service.ObtenerStocksPorReferenciaAsync());
        }

        [HttpGet("entradas")]
        public async Task<IActionResult> ObtenerEntradas()
        {
            return Ok(await _service.ObtenerEntradasAsync());
        }

        [HttpGet("mezclapororden")]
        public async Task<IActionResult> ObtenerMezclaPorOrden()
        {
            return Ok(await _service.ObtenerMezclaPorOrdenAsync());
        }

        [HttpGet("materiasprimas")]
        public async Task<IActionResult> ObtenerMateriasPrimas()
        {
            return Ok(await _service.ObtenerMateriasPrimasAsync());
        }

        [HttpGet("ordenesproduccion")]
        public async Task<IActionResult> ObtenerOrdenesProduccion()
        {
            return Ok(await _service.ObtenerOrdenesProduccionAsync());
        }

        [HttpGet("referencias")]
        public async Task<IActionResult> ObtenerReferencias()
        {
            return Ok(await _service.ObtenerReferenciasAsync());
        }

        [HttpGet("registroproduccion")]
        public async Task<IActionResult> ObtenerRegistroProduccion()
        {
            return Ok(await _service.ObtenerRegistroProduccionAsync());
        }
    }
}
