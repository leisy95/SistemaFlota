using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaFlota.Authorization;
using SistemaFlota.DTOs.ComprasNoFormalizadas.RecepcionMercancias;
using SistemaFlota.Services.ComprasNoFormalizadas.RecepcionMercancia;
using SistemaFlota.Services.ImpresionEtiquetas;

namespace SistemaFlota.Controllers.ComprasNoFormalizadas.RecepcionMercancias
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class RecepcionMercanciaNoFormalizadaController : ControllerBase
    {
        private readonly IRecepcionMercanciaNoFormalizadaService _service;
        private readonly IEtiquetasPdfService _etiquetasPdfService;

        public RecepcionMercanciaNoFormalizadaController(
            IRecepcionMercanciaNoFormalizadaService service,
            IEtiquetasPdfService etiquetasPdfService)
        {
            _service = service;
            _etiquetasPdfService = etiquetasPdfService;
        }

        // Listar recepciones
        [HttpGet]
        [Permiso("recepcion-mercancia-no-formalizada", "ver")]
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

        // Obtener recepción por ID
        [HttpGet("{id:int}")]
        [Permiso("recepcion-mercancia-no-formalizada", "ver")]
        public async Task<IActionResult> ObtenerPorId(int id)
        {
            var recepcion = await _service.ObtenerPorIdAsync(id);

            if (recepcion == null)
                return NotFound();

            return Ok(recepcion);
        }

        // Formulario de recepción
        [HttpGet("formulario/{ordenCompraNoFormalizadaId:int}")]
        [Permiso("recepcion-mercancia-no-formalizada", "ver")]
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

        // Imprimir etiquetas
        [HttpGet("{id}/etiquetas")]
        [Permiso("recepcion-mercancia-no-formalizada", "ver")]
        public async Task<IActionResult> ImprimirEtiquetas(int id)
        {
            var pdf = await _etiquetasPdfService.GenerarAsync(id);

            return File(
                pdf,
                "application/pdf",
                $"Etiquetas_NoFormalizada_{id}.pdf");
        }

        // Crear recepción
        [HttpPost]
        [Permiso("recepcion-mercancia-no-formalizada", "crear")]
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

        // Confirmar recepción de mercancía
        [HttpPut("{id}/confirmar")]
        [Permiso("recepcion-mercancia-no-formalizada", "editar")]
        public async Task<IActionResult> ConfirmarRecepcion(int id)
        {
            await _service.ConfirmarRecepcionAsync(id);

            return Ok(new
            {
                mensaje = "Recepción no formalizada confirmada correctamente."
            });
        }

        // Actualizar recepción
        [HttpPut("{id:int}")]
        [Permiso("recepcion-mercancia-no-formalizada", "editar")]
        public async Task<IActionResult> Actualizar(
            int id,
            [FromBody] ActualizarRecepcionMercanciaNoFormalizadaDto dto)
        {
            var actualizado = await _service.ActualizarAsync(id, dto);

            if (!actualizado)
                return NotFound();

            return NoContent();
        }

        // Eliminar recepción
        [HttpDelete("{id:int}")]
        [Permiso("recepcion-mercancia-no-formalizada", "eliminar")]
        public async Task<IActionResult> Eliminar(int id)
        {
            var eliminado = await _service.EliminarAsync(id);

            if (!eliminado)
                return NotFound();

            return NoContent();
        }

        // Filtros de recepción
        [HttpGet("filtros")]
        [Permiso("recepcion-mercancia-no-formalizada", "ver")]
        public async Task<IActionResult> ObtenerFiltros()
        {
            var filtros = await _service.ObtenerFiltrosAsync();

            return Ok(filtros);
        }
    }
}
