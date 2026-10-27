
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SistemaFlota.DTOs;
using SistemaFlota.Services.Calidad.Formatos;

namespace SistemaFlota.Controllers.Calidad.Formatos
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class FormatosCalidadController : ControllerBase
    {
        private readonly FormatosCalidadService _service;

        public FormatosCalidadController(FormatosCalidadService service)
        {
            _service = service;
        }

        // GET api/FormatosCalidad/tipos
        [HttpGet("tipos")]
        public async Task<IActionResult> GetTipos()
        {
            var resultado = await _service.ObtenerTiposAsync();
            return Ok(resultado);
        }

        // GET api/FormatosCalidad/tipos/{codigo}/caracteristicas
        [HttpGet("tipos/{codigo}/caracteristicas")]
        public async Task<IActionResult> GetCaracteristicas(string codigo)
        {
            var resultado = await _service.ObtenerCaracteristicasAsync(codigo);

            if (resultado == null)
                return NotFound();

            return Ok(resultado);
        }

        // GET api/FormatosCalidad/registros
        // GET api/FormatosCalidad/registros
        [HttpGet("registros")]
        public async Task<IActionResult> GetRegistros(
            [FromQuery] string codigo,
            [FromQuery] DateTime? desde,
            [FromQuery] DateTime? hasta,
            [FromQuery] string? op)
        {
            var resultado = await _service.ObtenerRegistrosAsync(
                codigo, desde, hasta, op);

            if (!resultado.TipoEncontrado)
            {
                return NotFound(new
                {
                    mensaje = "Tipo de formato no encontrado"
                });
            }

            return Ok(resultado.Registros);
        }

        // POST api/FormatosCalidad/registros
        [HttpPost("registros")]
        public async Task<IActionResult> Post(
            [FromBody] RegistroFormatoCalidadDto dto)
        {
            var resultado = await _service.CrearAsync(dto);

            if (resultado.StatusCode != StatusCodes.Status200OK)
            {
                return StatusCode(resultado.StatusCode, new
                {
                    mensaje = resultado.Mensaje
                });
            }

            return Ok(resultado.Registro);
        }

        // PUT api/FormatosCalidad/registros/{id}
        [HttpPut("registros/{id}")]
        public async Task<IActionResult> Put(
            int id,
            [FromBody] RegistroFormatoCalidadDto dto)
        {
            var registro = await _service.ActualizarAsync(id, dto);

            if (registro == null)
                return NotFound();

            return Ok(registro);
        }

        // PUT api/FormatosCalidad/registros/{id}/liberar
        [HttpPut("registros/{id}/liberar")]
        public async Task<IActionResult> Liberar(
            int id,
            [FromBody] LiberarFormatoDto dto)
        {
            var registro = await _service.LiberarAsync(id, dto);

            if (registro == null)
                return NotFound();

            return Ok(registro);
        }

        // GET api/FormatosCalidad/registros/op/{op}?tipoFormatoId=1
        [HttpGet("registros/op/{op}")]
        public async Task<IActionResult> GetPorOP(
            string op,
            [FromQuery] int tipoFormatoId)
        {
            var registro = await _service.ObtenerPorOPAsync(
                op, tipoFormatoId);

            return Ok(registro);
        }

        // GET api/FormatosCalidad/mezcla/{op}
        [HttpGet("mezcla/{op}")]
        public async Task<IActionResult> GetMezcla(string op)
        {
            var resultado = await _service.ObtenerMezclaAsync(op);
            return Ok(resultado);
        }

        // DELETE api/FormatosCalidad/registros/{id}
        [HttpDelete("registros/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var eliminado = await _service.EliminarAsync(id);

            if (!eliminado)
                return NotFound();

            return Ok();
        }

        // GET api/FormatosCalidad/desperdicio-orden
        [HttpGet("desperdicio-orden")]
        public async Task<IActionResult> DesperdicioPorOrden(
            [FromQuery] string op,
            [FromQuery] int tipoFormatoId)
        {
            if (string.IsNullOrWhiteSpace(op))
            {
                return BadRequest(new
                {
                    mensaje = "Debe indicar el número de orden"
                });
            }

            var resultado = await _service.ObtenerDesperdicioPorOrdenAsync(
                op, tipoFormatoId);

            return Ok(new
            {
                ordenProduccion = op,
                totalDesperdicio = resultado.TotalDesperdicio,
                totalRegistros = resultado.TotalRegistros
            });
        }

        // GET api/FormatosCalidad/mejor-rendimiento
        [HttpGet("mejor-rendimiento")]
        public async Task<IActionResult> MejorRendimiento(
            [FromQuery] string texto,
            [FromQuery] string? maquina = null)
        {
            if (string.IsNullOrWhiteSpace(texto))
            {
                return BadRequest(new
                {
                    mensaje = "Debe indicar un texto para buscar"
                });
            }

            var resultado = await _service.ObtenerMejorRendimientoAsync(
                texto, maquina);

            return StatusCode(
                resultado.StatusCode,
                resultado.Resultado);
        }

        // POST api/FormatosCalidad/registros/{id}/parametros-operario
        [HttpPost("registros/{id}/parametros-operario")]
        public async Task<IActionResult> GuardarParametrosOperario(
            int id,
            [FromBody] GuardarParametrosOperarioDto dto)
        {
            await _service.GuardarParametrosOperarioAsync(id, dto);
            return Ok();
        }

        // GET api/FormatosCalidad/registros/{id}/parametros-operario
        [HttpGet("registros/{id}/parametros-operario")]
        public async Task<IActionResult> GetParametrosOperario(int id)
        {
            var resultado =
                await _service.ObtenerParametrosOperarioAsync(id);

            return Ok(resultado);
        }

        // POST api/FormatosCalidad/registros/{id}/rondas/{rondaId}/fotos
        [HttpPost("registros/{id}/rondas/{rondaId}/fotos")]
        public async Task<IActionResult> SubirFotosRonda(
            int id,
            string rondaId,
            [FromForm] List<IFormFile> fotos)
        {
            var resultado = await _service.SubirFotosRondaAsync(
                id, rondaId, fotos);

            if (resultado.StatusCode != StatusCodes.Status200OK)
            {
                return StatusCode(resultado.StatusCode, new
                {
                    mensaje = resultado.Mensaje
                });
            }

            return Ok(resultado.Fotos);
        }

        // GET api/FormatosCalidad/registros/{id}/fotos
        [HttpGet("registros/{id}/fotos")]
        public async Task<IActionResult> GetFotosRondas(int id)
        {
            var resultado = await _service.ObtenerFotosRondasAsync(id);
            return Ok(resultado);
        }

        // DELETE api/FormatosCalidad/registros/{id}/fotos/{fotoId}
        [HttpDelete("registros/{id}/fotos/{fotoId}")]
        public async Task<IActionResult> EliminarFotoRonda(
            int id,
            int fotoId)
        {
            var resultado = await _service.EliminarFotoRondaAsync(
                id, fotoId);

            if (!resultado.Exito)
            {
                if (resultado.StatusCode == StatusCodes.Status403Forbidden)
                {
                    return StatusCode(
                        StatusCodes.Status403Forbidden,
                        new { mensaje = resultado.Mensaje });
                }

                if (resultado.StatusCode == StatusCodes.Status404NotFound)
                    return NotFound();

                return StatusCode(
                    resultado.StatusCode,
                    new { mensaje = resultado.Mensaje });
            }

            return Ok();
        }

        // PUT api/FormatosCalidad/{id}/fijar-mejor
        [HttpPut("{id}/fijar-mejor")]
        public async Task<IActionResult> FijarMejorRendimiento(int id)
        {
            var registro =
                await _service.FijarMejorRendimientoAsync(id);

            if (registro == null)
                return NotFound();

            return Ok(registro);
        }

        // PUT api/FormatosCalidad/{id}/quitar-fijado
        [HttpPut("{id}/quitar-fijado")]
        public async Task<IActionResult> QuitarFijado(int id)
        {
            var registro = await _service.QuitarFijadoAsync(id);

            if (registro == null)
                return NotFound();

            return Ok(registro);
        }
    }
}
