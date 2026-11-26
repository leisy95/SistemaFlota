namespace SistemaFlota.Models.Referencias
{
    public enum MaterialDescripcion
    {
        Original,
        PostIndustrial,
        Posconsumo,
        Compostable,
        Mezcla
    }
    public enum TroquelDescripcion
    {
        Ovalo,
        Gancho,
        Asa
    }

    public sealed class DescripcionInterpretada
    {
        public string Descripcion { get; init; } = string.Empty;
        public bool Valida { get; init; }
        public IReadOnlyList<string> Errores { get; init; } = Array.Empty<string>();
        public IReadOnlyList<string> Advertencias { get; init; } = Array.Empty<string>();

      
        public MaterialDescripcion? Material { get; init; }
        public bool Biodegradable { get; init; }
        public bool Impreso { get; init; }

       
        public decimal? FuelleLateral { get; init; }
        public decimal? FuelleFondo { get; init; }
        public decimal? FuelleSuperior { get; init; }
        public decimal? Solapa { get; init; }

        
        public TroquelDescripcion? Troquel { get; init; }

        
        public bool CintaSeguridad { get; init; }
        public bool CintaPegaDespega { get; init; }

        
        public IReadOnlyList<string> CodigosSinDefinir { get; init; } = Array.Empty<string>();
    }
}