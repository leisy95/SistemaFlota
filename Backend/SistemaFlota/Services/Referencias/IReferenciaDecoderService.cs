using SistemaFlota.Models.Referencias;

namespace SistemaFlota.Services.Referencias
{
    public interface IReferenciaDecoderService
    {
       
        ReferenciaDecodificada Decodificar(string? codigo);

        IReadOnlyList<ReferenciaDecodificada> DecodificarLote(IEnumerable<string?> codigos);
    }
}
