using SistemaFlota.DTOs.Referencias;
using SistemaFlota.Models.Referencias;

namespace SistemaFlota.Services.Referencias
{
    public static class ReferenciaDecodificadaMapper
    {
        public static ReferenciaDecodificadaDto ToDto(this ReferenciaDecodificada r) => new()
        {
            Codigo = r.Codigo,
            Valida = r.Valida,
            Errores = r.Errores.ToList(),
            Advertencias = r.Advertencias.ToList(),
            TipoCodigo = r.Tipo.HasValue ? (int)r.Tipo.Value : null,
            Tipo = r.Tipo.HasValue ? CatalogoReferencias.Tipos[r.Tipo.Value] : null,
            Origen = r.Origen?.ToString(),
            Densidad = r.Densidad?.ToString(),
            ColorCodigo = r.ColorCodigo,
            ColorNombre = r.ColorNombre,
            Impreso = r.Impreso,
            Ancho = r.Ancho,
            Largo = r.Largo,
            Calibre = r.Calibre,
            Sufijo = r.Sufijo,
            SufijoNombre = r.SufijoNombre,
            SufijoReconocido = r.SufijoReconocido,
            EsPosconsumo = r.EsPosconsumo,
            EsBiodegradable = r.EsBiodegradable,
            EsCompostable = r.EsCompostable
        };

        public static CatalogosReferenciaDto ToCatalogosDto() => new()
        {
            Tipos = CatalogoReferencias.Tipos
                .OrderBy(t => (int)t.Key)
                .Select(t => new ItemCatalogoReferenciaDto { Codigo = ((int)t.Key).ToString(), Nombre = t.Value }).ToList(),
            Colores = CatalogoReferencias.Colores
                .Select(c => new ItemCatalogoReferenciaDto { Codigo = c.Key, Nombre = c.Value }).ToList(),
            Sufijos = CatalogoReferencias.Sufijos
                .Select(s => new ItemCatalogoReferenciaDto { Codigo = s.Key, Nombre = s.Value }).ToList()
        };
    }
}
