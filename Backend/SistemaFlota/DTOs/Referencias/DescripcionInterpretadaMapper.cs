using SistemaFlota.DTOs.Referencias;
using SistemaFlota.Models.Referencias;

namespace SistemaFlota.Services.Referencias
{
    public static class DescripcionInterpretadaMapper
    {
        public static DescripcionInterpretadaDto ToDto(this DescripcionInterpretada d) => new()
        {
            Descripcion = d.Descripcion,
            Valida = d.Valida,
            Errores = d.Errores.ToList(),
            Advertencias = d.Advertencias.ToList(),
            Material = d.Material?.ToString(),
            Biodegradable = d.Biodegradable,
            Impreso = d.Impreso,
            FuelleLateral = d.FuelleLateral,
            FuelleFondo = d.FuelleFondo,
            FuelleSuperior = d.FuelleSuperior,
            Solapa = d.Solapa,
            Troquel = d.Troquel?.ToString(),
            CintaSeguridad = d.CintaSeguridad,
            CintaPegaDespega = d.CintaPegaDespega,
            CodigosSinDefinir = d.CodigosSinDefinir.ToList()
        };
    }
}
