
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaFlota.DTOs.ControlEnvios.Trazabilidad;
using SistemaFlota.Services.ControlEnvios.Trazabilidad;
using SistemaFlota.Services.ControlEnvios.Trazabilidad.TrazabilidadPdf;
using System.Security.Claims;

namespace SistemaFlota.Controllers.ControlEnvios.Trazabilidad
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TrazabilidadController : ControllerBase
    {
        private readonly ITrazabilidadService _service;
        private readonly ITrazabilidadPdfService _pdfService;
        public TrazabilidadController(
            ITrazabilidadService service,
            ITrazabilidadPdfService pdfService)
        {
            _service = service;
            _pdfService = pdfService;
        }

        private string GetUsuario() =>
            User.FindFirst(ClaimTypes.Name)?.Value ?? "Desconocido";

        private string GetRol() =>
            User.FindFirst(ClaimTypes.Role)?.Value ?? "Desconocido";

        // GET: api/Trazabilidad

        // GET: api/Trazabilidad
        [HttpGet]
        public async Task<IActionResult> Get(
            [FromQuery] int pagina = 1,
            [FromQuery] int porPagina = 50,
            [FromQuery] string? buscar = null,
            [FromQuery] string? estado = null,
            [FromQuery] string? entregada = null,
            [FromQuery] string? tipo = null,
            [FromQuery] DateTime? fechaDesde = null,
            [FromQuery] DateTime? fechaHasta = null)
        {
            var resultado = await _service.ObtenerAsync(
                pagina,
                porPagina,
                buscar,
                estado,
                entregada,
                tipo,
                fechaDesde,
                fechaHasta);

            return Ok(new
            {
                data = resultado.Data,
                total = resultado.Total,
                pagina = resultado.Pagina,
                porPagina = resultado.PorPagina,
                totalPaginas = resultado.TotalPaginas
            });
        }

        // GET: api/Trazabilidad/resumen
        [HttpGet("resumen")]
        public async Task<IActionResult> GetResumen(
            [FromQuery] string? buscar = null,
            [FromQuery] string? estado = null,
            [FromQuery] string? entregada = null,
            [FromQuery] string? tipo = null,
            [FromQuery] DateTime? fechaDesde = null,
            [FromQuery] DateTime? fechaHasta = null)
        {
            var resultado = await _service.ObtenerResumenAsync(
                buscar,
                estado,
                entregada,
                tipo,
                fechaDesde,
                fechaHasta);

            return Ok(new
            {
                total = resultado.Total,
                entregadas = resultado.Entregadas,
                pendientesEntrega = resultado.PendientesEntrega,
                totalFlete = resultado.TotalFlete
            });
        }

        // Imprimir Pdf general por filtros

        [HttpGet("exportar-pdf")]
        public async Task<IActionResult> ExportarPdf(
            [FromQuery] string? buscar = null,
            [FromQuery] string? estado = null,
            [FromQuery] string? entregada = null,
            [FromQuery] string? tipo = null,
            [FromQuery] DateTime? fechaDesde = null,
            [FromQuery] DateTime? fechaHasta = null)
        {
            var pdf = await _pdfService.GenerarPdfAsync(
                buscar,
                estado,
                entregada,
                tipo,
                fechaDesde,
                fechaHasta);

            return File(
                pdf,
                "application/pdf",
                $"Reporte_Trazabilidad_{DateTime.Now:yyyyMMdd_HHmmss}.pdf");
        }

        // GET: api/Trazabilidad/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var trazabilidad = await _service.ObtenerPorIdAsync(id);

            if (trazabilidad == null)
                return NotFound();

            return Ok(trazabilidad);
        }

        // POST: api/Trazabilidad
        [HttpPost]
        public async Task<IActionResult> Post(
            [FromBody] CrearTrazabilidadDto dto)
        {
            var resultado = await _service.CrearAsync(
                dto, GetUsuario(), GetRol());

            return Ok(resultado);
        }

        // PUT: api/Trazabilidad/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(
            int id,
            [FromBody] CrearTrazabilidadDto dto)
        {
            var resultado = await _service.ActualizarAsync(
                id, dto, GetUsuario(), GetRol());

            if (resultado == null)
                return NotFound();

            return Ok(resultado);
        }

        // DELETE: api/Trazabilidad/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var eliminado = await _service.EliminarAsync(
                id, GetUsuario(), GetRol());

            if (!eliminado)
                return NotFound();

            return Ok();
        }

        // GET: api/Trazabilidad/{id}/notas
        [HttpGet("{id}/notas")]
        public async Task<IActionResult> GetNotas(int id)
        {
            var notas = await _service.ObtenerNotasAsync(id);

            return Ok(notas);
        }

        // POST: api/Trazabilidad/{id}/notas
        [HttpPost("{id}/notas")]
        public async Task<IActionResult> AgregarNota(
            int id,
            [FromBody] CrearNotaDto dto)
        {
            var nota = await _service.AgregarNotaAsync(
                id, dto, GetUsuario(), GetRol());

            if (nota == null)
                return NotFound();

            return Ok(nota);
        }

        // PUT: api/Trazabilidad/notas/{notaId}
        [HttpPut("notas/{notaId}")]
        public async Task<IActionResult> EditarNota(
            int notaId,
            [FromBody] CrearNotaDto dto)
        {
            var nota = await _service.EditarNotaAsync(notaId, dto);

            if (nota == null)
                return NotFound();

            return Ok(nota);
        }

        // DELETE: api/Trazabilidad/notas/{notaId}
        [HttpDelete("notas/{notaId}")]
        public async Task<IActionResult> EliminarNota(int notaId)
        {
            var eliminado = await _service.EliminarNotaAsync(notaId);

            if (!eliminado)
                return NotFound();

            return Ok();
        }

        // GET: api/Trazabilidad/autorizaciones-disponibles
        [HttpGet("autorizaciones-disponibles")]
        public async Task<IActionResult> GetAutorizacionesDisponibles()
        {
            var lista =
                await _service.ObtenerAutorizacionesDisponiblesAsync();

            return Ok(lista);
        }
    }
}
