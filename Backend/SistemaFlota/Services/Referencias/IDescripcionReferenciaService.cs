using SistemaFlota.Models.Referencias;

namespace SistemaFlota.Services.Referencias
{
    public interface IDescripcionReferenciaService
    {
       
        DescripcionInterpretada Interpretar(string? descripcion);

        IReadOnlyList<DescripcionInterpretada> InterpretarLote(IEnumerable<string?> descripciones);
    }
}
