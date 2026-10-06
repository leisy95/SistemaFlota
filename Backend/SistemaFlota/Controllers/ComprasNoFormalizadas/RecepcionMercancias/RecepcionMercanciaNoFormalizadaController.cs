using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaFlota.Authorization;
using SistemaFlota.DTOs.ComprasNoFormalizadas.RecepcionMercancias;
using SistemaFlota.Services.ComprasNoFormalizadas.ImpresionEtiquetasNoFormalizadas;
using SistemaFlota.Services.ComprasNoFormalizadas.RecepcionMercancia;

namespace SistemaFlota.Controllers.ComprasNoFormalizadas.RecepcionMercancias
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class RecepcionMercanciaNoFormalizadaController : ControllerBase
    {
        private readonly IRecepcionMercanciaNoFormalizadaService _service;
        private readonly IEtiquetasPdfNoFormalizadaService _etiquetasPdfService;
        private readonly IRecepcionMercanciaNoFormalizadaPdfService _recepcionMercanciaNoFormalizadaPdfService;

        public RecepcionMercanciaNoFormalizadaController(
            IRecepcionMercanciaNoFormalizadaService service,
            IEtiquetasPdfNoFormalizadaService etiquetasPdfService,
            IRecepcionMercanciaNoFormalizadaPdfService recepcionMercanciaNoFormalizadaPdfService)
        {
            _service = service;
            _etiquetasPdfService = etiquetasPdfService;
            _recepcionMercanciaNoFormalizadaPdfService = recepcionMercanciaNoFormalizadaPdfService;
        }

        [HttpGet]
        [Permiso("recepcion-compras-no-formalizadas", "ver")]
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
        [Permiso("recepcion-compras-no-formalizadas", "ver")]
        public async Task<IActionResult> ObtenerPorId(int id)
        {
            var recepcion = await _service.ObtenerPorIdAsync(id);

            if (recepcion == null)
                return NotFound();

            return Ok(recepcion);
        }

        [HttpGet("formulario/{ordenCompraNoFormalizadaId:int}")]
        [Permiso("recepcion-compras-no-formalizadas", "ver")]
        public async Task<IActionResult> ObtenerFormulario(
            int ordenCompraNoFormalizadaId)
        {
            try
            {
                var formulario = await _service.ObtenerFormularioAsync(
                    ordenCompraNoFormalizadaId);

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

        [HttpGet("{id:int}/pdf")]
        [Permiso("recepcion-compras-no-formalizadas", "ver")]
        public async Task<IActionResult> ObtenerPdf(int id)
        {
            try
            {
                var pdf = await _recepcionMercanciaNoFormalizadaPdfService
                    .GenerarPdfAsync(id);

                return File(
                    pdf,
                    "application/pdf",
                    $"Recepcion_NoFormalizada_{id}.pdf");
            }
            catch (Exception ex)
            {
                return NotFound(new
                {
                    mensaje = ex.Message
                });
            }
        }

        [HttpGet("{id:int}/etiquetas")]
        [Permiso("recepcion-compras-no-formalizadas", "ver")]
        public async Task<IActionResult> ImprimirEtiquetas(
            int id,
            [FromQuery] int numeroEntrega)
        {
            var pdf = await _etiquetasPdfService.GenerarAsync(id);

            return File(
                pdf,
                "application/pdf",
                $"Etiquetas_NoFormalizada_{id}_Entrega_{numeroEntrega}.pdf");
        }

        [HttpPost]
        [Permiso("recepcion-compras-no-formalizadas", "crear")]
        public async Task<IActionResult> Crear(
            [FromBody] CrearRecepcionMercanciaNoFormalizadaDto dto)
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

        [HttpPut("{id:int}/confirmar")]
        [Permiso("recepcion-compras-no-formalizadas", "editar")]
        public async Task<IActionResult> ConfirmarRecepcion(int id)
        {
            await _service.ConfirmarRecepcionAsync(id);

            return Ok(new
            {
                mensaje = "Recepción no formalizada confirmada correctamente."
            });
        }

        [HttpPut("{id:int}")]
        [Permiso("recepcion-compras-no-formalizadas", "editar")]
        public async Task<IActionResult> Actualizar(
            int id,
            [FromBody] ActualizarRecepcionMercanciaNoFormalizadaDto dto)
        {
            var actualizado = await _service.ActualizarAsync(id, dto);

            if (!actualizado)
                return NotFound();

            return NoContent();
        }

        [HttpDelete("{id:int}")]
        [Permiso("recepcion-compras-no-formalizadas", "eliminar")]
        public async Task<IActionResult> Eliminar(int id)
        {
            var eliminado = await _service.EliminarAsync(id);

            if (!eliminado)
                return NotFound();

            return NoContent();
        }

        [HttpGet("filtros")]
        [Permiso("recepcion-compras-no-formalizadas", "ver")]
        public async Task<IActionResult> ObtenerFiltros()
        {
            var filtros = await _service.ObtenerFiltrosAsync();

            return Ok(filtros);
        }
    }
}