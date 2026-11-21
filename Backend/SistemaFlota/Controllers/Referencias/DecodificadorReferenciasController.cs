using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaFlota.DTOs.Referencias;
using SistemaFlota.Services.Referencias;

namespace SistemaFlota
{
    
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DecodificadorReferenciasController : ControllerBase
    {
        private readonly IReferenciaDecoderService _decoder;
        private readonly IDescripcionReferenciaService _descripciones;

        public DecodificadorReferenciasController(IReferenciaDecoderService decoder, IDescripcionReferenciaService descripciones)
        {
            _decoder = decoder;
            _descripciones = descripciones;
        }

       
        [HttpPost("decodificar")]
        [RequestSizeLimit(4 * 1024)]
        public ActionResult<ReferenciaDecodificadaDto> Decodificar([FromBody] DecodificarReferenciaRequestDto dto)
        {
            return Ok(_decoder.Decodificar(dto.Codigo).ToDto());
        }

        
        [HttpPost("decodificar-lote")]
        [RequestSizeLimit(32 * 1024)]
        public ActionResult<List<ReferenciaDecodificadaDto>> DecodificarLote([FromBody] DecodificarReferenciasLoteRequestDto dto)
        {
            return Ok(_decoder.DecodificarLote(dto.Codigos).Select(r => r.ToDto()).ToList());
        }

       
        [HttpPost("interpretar-descripcion")]
        [RequestSizeLimit(4 * 1024)]
        public ActionResult<DescripcionInterpretadaDto> InterpretarDescripcion([FromBody] InterpretarDescripcionRequestDto dto)
        {
            return Ok(_descripciones.Interpretar(dto.Descripcion).ToDto());
        }

       
        [HttpPost("interpretar-descripciones-lote")]
        [RequestSizeLimit(128 * 1024)]
        public ActionResult<List<DescripcionInterpretadaDto>> InterpretarDescripcionesLote([FromBody] InterpretarDescripcionesLoteRequestDto dto)
        {
            return Ok(_descripciones.InterpretarLote(dto.Descripciones).Select(d => d.ToDto()).ToList());
        }

       
        [HttpGet("catalogos")]
        public ActionResult<CatalogosReferenciaDto> Catalogos()
        {
            return Ok(ReferenciaDecodificadaMapper.ToCatalogosDto());
        }
    }
}