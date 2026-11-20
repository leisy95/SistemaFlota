
using Microsoft.AspNetCore.Mvc;
using SistemaFlota.Services.Produccion.LecturaImagen;

namespace SistemaFlota.Controllers.Produccion.LecturaImagen
{
    [ApiController]
    [Route("api/produccion/retal")]
    public class RetalController : ControllerBase
    {
        private readonly ILectorReferenciaService _lectorService;

        public RetalController(
            ILectorReferenciaService lectorService)
        {
            _lectorService = lectorService;
        }

        [HttpPost("leer-referencia")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> LeerReferencia(IFormFile imagen)
        {
            Console.WriteLine("CONTROLADOR: Llegó una petición para leer la referencia.");

            if (imagen == null || imagen.Length == 0)
                return BadRequest("Debe seleccionar una imagen.");

            try
            {
                Console.WriteLine("CONTROLADOR: Enviando imagen al servicio OCR.");

                var resultado = await _lectorService.LeerReferenciaAsync(imagen);

                Console.WriteLine("CONTROLADOR: El servicio OCR terminó.");

                if (!resultado.LeidaCorrectamente)
                    return BadRequest(resultado);

                return Ok(resultado);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"CONTROLADOR ERROR: {ex}");

                return StatusCode(500, new
                {
                    mensaje = "Error al procesar la imagen.",
                    detalle = ex.Message
                });
            }
        }
    }
}
