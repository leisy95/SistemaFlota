using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaFlota.DTOs.Costos.RecepcionMercancia;
using SistemaFlota.Services.Costos.RecepcionMercancia;
using SistemaFlota.Services.ImpresionEtiquetas;

namespace SistemaFlota.Controllers.Costos.RecepcionMercancia
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class RecepcionMercanciaController : ControllerBase
    {
        private readonly IRecepcionMercanciaService _service;
        private readonly IEtiquetasPdfService _etiquetasPdfService;

        public RecepcionMercanciaController(
            IRecepcionMercanciaService service,
            IEtiquetasPdfService etiquetasPdfService)
        {
            _service = service;
            _etiquetasPdfService = etiquetasPdfService;
        }

        [HttpGet]
        public async Task<IActionResult> Obtener(
            [FromQuery] string? search,
            [FromQuery] DateTime? fechaInicio,
            [FromQuery] DateTime? fechaFin,
            [FromQuery] int? proveedorId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            var resultado = await _service.ObtenerAsync(
                search,
                fechaInicio,
                fechaFin,
                proveedorId,
                page,
                pageSize);

            return Ok(resultado);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> ObtenerPorId(int id)
        {
            var recepcion = await _service.ObtenerPorIdAsync(id);

            if (recepcion == null)
                return NotFound();

            return Ok(recepcion);
        }

        [HttpGet("formulario/{ordenCompraId:int}")]
        public async Task<IActionResult> ObtenerFormulario(int ordenCompraId)
        {
            try
            {
                var formulario = await _service.ObtenerFormularioAsync(ordenCompraId);

                if (formulario == null)
                    return NotFound();

                return Ok(formulario);
            }
            catch (Exception ex)
            {
                return Conflict(new
                {
                    mensaje = ex.Message
                });
            }
        }

        [HttpGet("{id}/etiquetas")]
        public async Task<IActionResult> ImprimirEtiquetas(int id)
        {
            var pdf = await _etiquetasPdfService.GenerarAsync(id);

            return File(
                pdf,
                "application/pdf",
                $"Etiquetas_{id}.pdf");
        }

        [HttpPost]
        public async Task<IActionResult> Crear([FromBody] CrearRecepcionMercanciaDto dto)
        {
            try
            {
                var recepcion = await _service.CrearAsync(dto);

                return Ok(recepcion);
            }
            catch (Exception ex)
            {
                return Conflict(new
                {
                    mensaje = ex.Message
                });
            }
        }

        // Confirmar recepcion de mercancia
        [HttpPut("{id}/confirmar")]
        public async Task<IActionResult> ConfirmarRecepcion(int id)
        {
            await _service.ConfirmarRecepcionAsync(id);

            return Ok(new
            {
                mensaje = "Recepción confirmada correctamente."
            });
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Actualizar(
            int id,
            [FromBody] ActualizarRecepcionMercanciaDto dto)
        {
            var actualizado = await _service.ActualizarAsync(id, dto);

            if (!actualizado)
                return NotFound();

            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Eliminar(int id)
        {
            var eliminado = await _service.EliminarAsync(id);

            if (!eliminado)
                return NotFound();

            return NoContent();
        }

        [HttpGet("filtros")]
        public async Task<IActionResult> ObtenerFiltros()
        {
            var filtros = await _service.ObtenerFiltrosAsync();

            return Ok(filtros);
        }
    }
}