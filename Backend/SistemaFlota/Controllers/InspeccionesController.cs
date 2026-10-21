
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaFlota.DTOs.Flota.Inspecciones;
using SistemaFlota.Services.Flota.Inspecciones;
using SistemaFlota.Services.Flota.Inspecciones.InspecionPdfGeneral;
using System.Security.Claims;

namespace SistemaFlota
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class InspeccionesController : ControllerBase
    {
        private readonly IInspeccionService _inspeccionService;
        private readonly IInspeccionPdfService _inspeccionPdfService;
        private readonly IInspeccionPdfGeneralService _pdfGeneralService;

        public InspeccionesController(
            IInspeccionService inspeccionService,
            IInspeccionPdfService inspeccionPdfService,
            IInspeccionPdfGeneralService pdfGeneralService)
        {
            _inspeccionService = inspeccionService;
            _inspeccionPdfService = inspeccionPdfService;
            _pdfGeneralService = pdfGeneralService;
        }

        private string GetUsuario() =>
            User.FindFirst(ClaimTypes.Name)?.Value ?? "Desconocido";

        private string GetRol() =>
            User.FindFirst(ClaimTypes.Role)?.Value ?? "Desconocido";

        // GET: api/Inspecciones

        [HttpGet]
        public async Task<IActionResult> Get(
            [FromQuery] int pagina = 1,
            [FromQuery] int porPagina = 20,
            [FromQuery] string? buscar = null,
            [FromQuery] int? conductorId = null,
            [FromQuery] int? vehiculoId = null,
            [FromQuery] DateTime? fechaDesde = null,
            [FromQuery] DateTime? fechaHasta = null)
        {
            if (fechaDesde.HasValue &&
                fechaHasta.HasValue &&
                fechaDesde.Value.Date > fechaHasta.Value.Date)
            {
                return BadRequest(
                    "La fecha inicial no puede ser posterior a la fecha final.");
            }

            var resultado = await _inspeccionService.ObtenerInspeccionesAsync(
                pagina,
                porPagina,
                buscar,
                conductorId,
                vehiculoId,
                fechaDesde,
                fechaHasta);

            return Ok(resultado);
        }

        // GET: api/Inspecciones/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetDetalle(int id)
        {
            var resultado = await _inspeccionService.ObtenerDetalleAsync(id);

            if (resultado == null)
                return NotFound();

            return Ok(resultado);
        }

        // Pdf para inpecciones

        [HttpGet("{id}/pdf")]
        public async Task<IActionResult> ExportarPdf(int id)
        {
            Console.WriteLine($"Endpoint PDF ejecutado. ID: {id}");

            var pdf = await _inspeccionPdfService.GenerarPdfAsync(id);

            Console.WriteLine($"Servicio PDF ejecutado. ID: {id}");

            return File(
                pdf,
                "application/pdf",
                $"Inspeccion-{id}.pdf"
            );
        }


        // Inspecciones pdf-general
        [HttpGet("pdf-general")]
        public async Task<IActionResult> ExportarPdfGeneral(
            [FromQuery] DateTime? fechaDesde = null,
            [FromQuery] DateTime? fechaHasta = null,
            [FromQuery] string? buscar = null)
        {
            if (fechaDesde.HasValue &&
                fechaHasta.HasValue &&
                fechaDesde.Value.Date > fechaHasta.Value.Date)
            {
                return BadRequest(
                    "La fecha inicial no puede ser posterior a la fecha final.");
            }

            var pdf = await _pdfGeneralService.GenerarPdfGeneralAsync(
                buscar,
                fechaDesde,
                fechaHasta);

            return File(
                pdf,
                "application/pdf",
                "historial-inspecciones.pdf");
        }

        // POST: api/Inspecciones
        [HttpPost]
        public async Task<IActionResult> Post(
            [FromForm] CrearInspeccionDto dto)
        {
            try
            {
                var resultado = await _inspeccionService.CrearAsync(
                    dto,
                    GetUsuario(),
                    GetRol());

                return Ok(resultado);
            }
            catch (Exception)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    "Ocurrió un error al crear la inspección.");
            }
        }
    }
}
