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

        public DecodificadorReferenciasController(IReferenciaDecoderService decoder)
        {
            _decoder = decoder;
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

        
        [HttpGet("catalogos")]
        public ActionResult<CatalogosReferenciaDto> Catalogos()
        {
            return Ok(ReferenciaDecodificadaMapper.ToCatalogosDto());
        }
    }
}