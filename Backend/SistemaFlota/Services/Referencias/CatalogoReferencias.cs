using SistemaFlota.Models.Referencias;

namespace SistemaFlota.Services.Referencias
{
   
    public static class CatalogoReferencias
    {
        public static readonly IReadOnlyDictionary<TipoProductoReferencia, string> Tipos =
            new Dictionary<TipoProductoReferencia, string>
            {
                [TipoProductoReferencia.Tubular] = "Tubular",
                [TipoProductoReferencia.Almacigo] = "Almácigo / vivero",
                [TipoProductoReferencia.BolsaSuelta] = "Bolsa suelta",
                [TipoProductoReferencia.Precorte] = "Precorte",
                [TipoProductoReferencia.Basura] = "Basura"
            };

        public static readonly IReadOnlyDictionary<string, string> Colores =
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["TR"] = "Transparente",
                ["BL"] = "Blanco",
                ["NE"] = "Negro",
                ["51"] = "Negro",
                ["RS"] = "Rosado",
                ["RO"] = "Rojo",
                ["AZ"] = "Azul",
                ["AM"] = "Amarillo",
                ["BE"] = "Beige",
                ["VE"] = "Verde",
                ["NA"] = "Naranja",
                ["FU"] = "Fucsia",
                ["MA"] = "Magenta",
                ["GR"] = "Gris",
                ["81"] = "Amarillo panadería",
                ["82"] = "Beige panadería"
            };

        
        public static readonly IReadOnlyDictionary<string, string> Sufijos =
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["P"] = "Posconsumo",
                ["B"] = "Biodegradable",
                ["E"] = "Compostable",
                ["TC"] = "Troquel camiseta",
                ["GZ"] = "Gancho",
                ["PQ"] = "Paquete"
            };
    }
}