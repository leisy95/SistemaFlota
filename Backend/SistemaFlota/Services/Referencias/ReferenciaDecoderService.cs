using System.Globalization;
using System.Text.RegularExpressions;
using SistemaFlota.Models.Referencias;

namespace SistemaFlota.Services.Referencias
{
   
    public class ReferenciaDecoderService : IReferenciaDecoderService
    {
        public const int LongitudMaxima = 40;

        private static readonly Regex Patron = new(
            @"^(?<tipo>[1-5])(?<origen>[RV])(?<dens>[0A])(?<color>[A-Z0-9]{2})(?<imp>[0-9])" +
            @"(?<ancho>\d{1,3}(?:\.\d{1,2})?)\*(?<largo>\d{1,3}(?:\.\d{1,2})?)\*(?<cal>\d{1,2}\.\d{1,2})" +
            @"(?<suf>[A-Z]{0,3})$",
            RegexOptions.Compiled | RegexOptions.CultureInvariant,
            TimeSpan.FromMilliseconds(100));

        public ReferenciaDecodificada Decodificar(string? codigo)
        {
            var limpio = (codigo ?? string.Empty).Trim().ToUpperInvariant();

            if (limpio.Length == 0)
                return Invalida(string.Empty, "La referencia está vacía.");

            if (limpio.Length > LongitudMaxima)
                return Invalida(string.Empty, $"La referencia supera {LongitudMaxima} caracteres.");

            Match m;
            try
            {
                m = Patron.Match(limpio);
            }
            catch (RegexMatchTimeoutException)
            {
                return Invalida(limpio, "No se pudo leer la referencia.");
            }

            if (!m.Success)
                return Invalida(limpio, "La referencia no tiene el formato esperado, por ejemplo 4R0TR9012.0*018.0*1.00P.");

            var advertencias = new List<string>();

            var tipo = (TipoProductoReferencia)int.Parse(m.Groups["tipo"].Value, CultureInfo.InvariantCulture);
            var origen = m.Groups["origen"].Value == "R" ? OrigenReferencia.Recuperado : OrigenReferencia.Original;
            var densidad = m.Groups["dens"].Value == "A" ? DensidadReferencia.Alta : DensidadReferencia.Baja;

            var colorCodigo = m.Groups["color"].Value;
            CatalogoReferencias.Colores.TryGetValue(colorCodigo, out var colorNombre);
            if (colorNombre is null)
                advertencias.Add($"El color {colorCodigo} no está en el catálogo.");

            var marcaImpreso = m.Groups["imp"].Value;
            if (marcaImpreso != "0" && marcaImpreso != "9")
                advertencias.Add($"El carácter 6 es {marcaImpreso}; se esperaba 0 o 9.");

            var sufijo = m.Groups["suf"].Value;
            string? sufijoNombre = null;
            var sufijoReconocido = false;
            if (sufijo.Length > 0)
            {
                sufijoReconocido = CatalogoReferencias.Sufijos.TryGetValue(sufijo, out sufijoNombre);
                if (!sufijoReconocido)
                    advertencias.Add($"El sufijo {sufijo} no está en el catálogo.");
            }

            return new ReferenciaDecodificada
            {
                Codigo = limpio,
                Valida = true,
                Advertencias = advertencias,
                Tipo = tipo,
                Origen = origen,
                Densidad = densidad,
                ColorCodigo = colorCodigo,
                ColorNombre = colorNombre,
                Impreso = marcaImpreso == "9",
                Ancho = Numero(m.Groups["ancho"].Value),
                Largo = Numero(m.Groups["largo"].Value),
                Calibre = Numero(m.Groups["cal"].Value),
                Sufijo = sufijo.Length > 0 ? sufijo : null,
                SufijoNombre = sufijoNombre,
                SufijoReconocido = sufijoReconocido
            };
        }

        public IReadOnlyList<ReferenciaDecodificada> DecodificarLote(IEnumerable<string?> codigos)
            => codigos.Select(Decodificar).ToList();

        private static decimal? Numero(string texto)
            => decimal.TryParse(texto, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var v) ? v : null;

        
        private static ReferenciaDecodificada Invalida(string codigo, string error)
            => new() { Codigo = codigo, Valida = false, Errores = new[] { error } };
    }
}