using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SistemaFlota.Authorization;
using SistemaFlota.DTOs.Calidad.FormatoFGC008;
using SistemaFlota.Services.Calidad.FormatoFGC008;
using SistemaFlota.Services.Calidad.FormatoFGC008.FormatosFGC008Pdf;
using SistemaFlota.Services.InformacionKasoft.OrdenesProduccion;
using System.Security.Claims;

namespace SistemaFlota
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class FormatoFGC008Controller : ControllerBase
    {
        private readonly IFormatoFGC008Service _service;
        private readonly AuditoriaService _auditoria;
        private readonly IOrdenProduccionService _ordenProduccionService;
        private readonly IFGC008PdfService _pdfService;

        public FormatoFGC008Controller(
            IFormatoFGC008Service service,
            AuditoriaService auditoria,
            IOrdenProduccionService ordenProduccionService,
            IFGC008PdfService pdfService)
        {
            _service = service;
            _auditoria = auditoria;
            _ordenProduccionService = ordenProduccionService;
            _pdfService = pdfService;
        }

        private string GetUsuario() =>
            User.FindFirst(ClaimTypes.Name)?.Value ?? "Desconocido";

        private string GetRol() =>
            User.FindFirst(ClaimTypes.Role)?.Value ?? "Desconocido";

        // Consultar todos los registros
        [HttpGet]
        [Permiso("calidad-formatos", "ver")]
        public async Task<IActionResult> Get(
            [FromQuery] DateTime? desde,
            [FromQuery] DateTime? hasta,
            [FromQuery] string? op)
        {
            var lista = await _service.ObtenerTodosAsync(desde, hasta, op);
            return Ok(lista);
        }

        // Consultar registros paginados
        [HttpGet("paginado")]
        [Permiso("calidad-formatos", "ver")]
        public async Task<IActionResult> GetPaginado(
            [FromQuery] DateTime? desde,
            [FromQuery] DateTime? hasta,
            [FromQuery] string? op,
            [FromQuery] int pagina = 1,
            [FromQuery] int porPagina = 25)
        {
            var resultado = await _service.ObtenerPaginadoAsync(
                desde, hasta, op, pagina, porPagina);

            return Ok(resultado);
        }

        // Generar PDF del formato F-GC-008
        [HttpGet("pdf")]
        [Permiso("calidad-formatos", "ver")]
        public async Task<IActionResult> GenerarPdf(
            [FromQuery] DateTime? desde,
            [FromQuery] DateTime? hasta,
            [FromQuery] string? op)
        {
            try
            {
                var pdf = await _pdfService.GenerarPdfAsync(
                    desde,
                    hasta,
                    op
                );

                return File(
                    pdf,
                    "application/pdf",
                    $"FGC008_{DateTime.Now:yyyyMMdd_HHmmss}.pdf"
                );
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new
                {
                    mensaje = ex.Message
                });
            }
        }

        // Resumen general
        [HttpGet("resumen")]
        [Permiso("calidad-formatos", "ver")]
        public async Task<IActionResult> GetResumen(
            [FromQuery] DateTime? desde,
            [FromQuery] DateTime? hasta,
            [FromQuery] string? op)
        {
            var resultado = await _service.ObtenerResumenAsync(desde, hasta, op);
            return Ok(resultado);
        }

        // Resumen por orden de producción
        [HttpGet("op/{op}")]
        [Permiso("calidad-formatos", "ver")]
        public async Task<IActionResult> GetPorOP(string op)
        {
            var registros = await _service.ObtenerPorOPAsync(op);

            if (registros.Count == 0)
                return NotFound();

            return Ok(new
            {
                cantidadOP = registros[0].CantidadOP,
                totalEntradas = registros.Count,
                totalReal = registros.Sum(f => f.CantidadReal)
            });
        }

        // Crear registro
        [HttpPost]
        [Permiso("calidad-formatos", "crear")]
        public async Task<IActionResult> Post(
            [FromForm] FormatoFGC008Dto dto,
            IFormFile? foto)
        {
            var resultado = await _service.CrearAsync(
                dto, GetUsuario(), foto);

            if (resultado.Duplicado)
            {
                return Conflict(new
                {
                    mensaje = "Ese registro ya se guardó hace un momento"
                });
            }

            var registro = resultado.Registro!;

            await _auditoria.RegistrarAsync(
                GetUsuario(),
                GetRol(),
                "Crear",
                "FormatoFGC008",
                $"Registro OP: {dto.OrdenProduccion}",
                registro.Id);

            return Ok(registro);
        }

        // Actualizar registro
        [HttpPut("{id}")]
        [Permiso("calidad-formatos", "editar")]
        public async Task<IActionResult> Put(
            int id,
            [FromForm] FormatoFGC008Dto dto,
            IFormFile? foto)
        {
            var actualizado = await _service.ActualizarAsync(
                id, dto, GetUsuario(), foto);

            if (!actualizado)
                return NotFound();

            var registro = await _service.ObtenerPorIdAsync(id);
            return Ok(registro);
        }

        // Subir fotos de evidencia
        [HttpPost("{id}/fotos")]
        [Permiso("calidad-formatos", "crear")]
        public async Task<IActionResult> SubirFotos(
            int id,
            [FromForm] List<IFormFile> fotos)
        {
            var resultado = await _service.SubirFotosAsync(
                id, fotos, GetUsuario());

            if (!resultado.Exito)
            {
                if (resultado.Mensaje == "Registro no encontrado")
                    return NotFound();

                return BadRequest(new { mensaje = resultado.Mensaje });
            }

            var usuario = GetUsuario();
            var registro = await _service.ObtenerPorIdAsync(id);

            await _auditoria.RegistrarAsync(
                usuario,
                GetRol(),
                "Subir-Fotos",
                "FormatoFGC008",
                $"{fotos.Count} foto(s) en OP {registro?.OrdenProduccion}",
                id);

            return Ok(resultado.Fotos);
        }

        // Eliminar foto de evidencia
        [HttpDelete("{id}/fotos/{fotoId}")]
        [Permiso("calidad-formatos", "crear")]
        public async Task<IActionResult> EliminarFoto(int id, int fotoId)
        {
            var eliminado = await _service.EliminarFotoAsync(id, fotoId);

            if (!eliminado)
                return NotFound();

            return Ok();
        }

        // Sincronizar desde Bodega
        [HttpPost("sincronizar/{op}")]
        [Permiso("calidad-formatos", "crear")]
        public async Task<IActionResult> Sincronizar(
            string op,
            [FromServices] SincronizacionFgc008Service sincronizacion)
        {
            var orden = await _ordenProduccionService.BuscarPorNumeroAsync(op);

            if (orden == null)
            {
                return NotFound(new
                {
                    mensaje = "No se encontró esa orden en producción"
                });
            }

            var resultado = await sincronizacion.SincronizarOpAsync(op);

            if (resultado.EntradasLeidas == 0)
            {
                return Ok(new
                {
                    mensaje = "La orden existe, pero Bodega todavía no tiene entradas para ella",
                    resultado
                });
            }

            await _auditoria.RegistrarAsync(
                GetUsuario(),
                GetRol(),
                "Sincronizar",
                "FormatoFGC008",
                $"OP {op}: {resultado.FilasCreadas} filas creadas, {resultado.FilasActualizadas} actualizadas");

            var filas = await _service.ObtenerPorOPAsync(op);

            return Ok(new { resultado, filas });
        }

        // Eliminar registro
        [HttpDelete("{id}")]
        [Permiso("calidad-formatos", "eliminar")]
        public async Task<IActionResult> Delete(int id)
        {
            var eliminado = await _service.EliminarAsync(id);

            if (!eliminado)
                return NotFound();

            return Ok();
        }
    }
}