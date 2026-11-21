namespace SistemaFlota.Models.Referencias
{
    public enum TipoProductoReferencia
    {
        Tubular = 1,
        Almacigo = 2,
        BolsaSuelta = 3,
        Precorte = 4,
        Basura = 5
    }

    public enum OrigenReferencia
    {
        Original,
        Recuperado
    }

    public enum DensidadReferencia
    {
        Baja,
        Alta
    }

   
    public sealed class ReferenciaDecodificada
    {
        public string Codigo { get; init; } = string.Empty;
        public bool Valida { get; init; }
        public IReadOnlyList<string> Errores { get; init; } = Array.Empty<string>();
        public IReadOnlyList<string> Advertencias { get; init; } = Array.Empty<string>();

        public TipoProductoReferencia? Tipo { get; init; }
        public OrigenReferencia? Origen { get; init; }
        public DensidadReferencia? Densidad { get; init; }

        public string? ColorCodigo { get; init; }
        public string? ColorNombre { get; init; }

        // Carácter 6 de la referencia: 9 = impreso.
        public bool Impreso { get; init; }

        public decimal? Ancho { get; init; }
        public decimal? Largo { get; init; }
        public decimal? Calibre { get; init; }

        public string? Sufijo { get; init; }
        public string? SufijoNombre { get; init; }
        public bool SufijoReconocido { get; init; }

        public bool EsPosconsumo => Sufijo == "P";
        public bool EsBiodegradable => Sufijo == "B";
        public bool EsCompostable => Sufijo == "E";
    }
}