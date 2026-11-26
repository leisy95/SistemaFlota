using System.Globalization;
using System.Text.RegularExpressions;
using SistemaFlota.Models.Referencias;

namespace SistemaFlota.Services.Referencias
{
   
    public class DescripcionReferenciaService : IDescripcionReferenciaService
    {
        public const int LongitudMaxima = 300;
        private const int MaxCodigosSinDefinir = 20;
        private const int LongitudMaxCodigo = 30;

        private static readonly TimeSpan Limite = TimeSpan.FromMilliseconds(100);
        private const RegexOptions Opc = RegexOptions.Compiled | RegexOptions.CultureInvariant;

        private static readonly Regex Parentesis = new(@"\(([^)]*)\)", Opc, Limite);
        private static readonly Regex Medida = new(@"^(FL|FF|FS|SL)(\d+(?:[.,]\d+)?)$", Opc, Limite);

        private static readonly Regex PalMezcla = new(@"\bMEZCLA\b", Opc, Limite);
        private static readonly Regex PalCompostable = new(@"\bCOMPOSTABLE\b", Opc, Limite);
        private static readonly Regex PalPosconsumo = new(@"\bPOSCONSUMO\b", Opc, Limite);
        private static readonly Regex PalPostIndustrial = new(@"\bPOSTINDUSTRIAL\b", Opc, Limite);
        private static readonly Regex PalOriginal = new(@"\bORIGINAL\b", Opc, Limite);
        private static readonly Regex PalBiodegradable = new(@"\bBIODEGRADABLE\b", Opc, Limite);
        private static readonly Regex PalImpreso = new(@"\bIMPRESO\b", Opc, Limite);

        public DescripcionInterpretada Interpretar(string? descripcion)
        {
            var limpio = (descripcion ?? string.Empty).Trim().ToUpperInvariant();

            if (limpio.Length == 0)
                return Invalida(string.Empty, "La descripción está vacía.");

            if (limpio.Length > LongitudMaxima)
                return Invalida(string.Empty, $"La descripción supera {LongitudMaxima} caracteres.");

            try
            {
                return Leer(limpio);
            }
            catch (RegexMatchTimeoutException)
            {
                return Invalida(string.Empty, "No se pudo leer la descripción.");
            }
        }

        public IReadOnlyList<DescripcionInterpretada> InterpretarLote(IEnumerable<string?> descripciones)
            => descripciones.Select(Interpretar).ToList();

        private static DescripcionInterpretada Leer(string texto)
        {
            var advertencias = new List<string>();

            MaterialDescripcion? material = null;
            if (PalMezcla.IsMatch(texto)) material = MaterialDescripcion.Mezcla;
            else if (PalCompostable.IsMatch(texto)) material = MaterialDescripcion.Compostable;
            else if (PalPosconsumo.IsMatch(texto)) material = MaterialDescripcion.Posconsumo;
            else if (PalPostIndustrial.IsMatch(texto)) material = MaterialDescripcion.PostIndustrial;
            else if (PalOriginal.IsMatch(texto)) material = MaterialDescripcion.Original;

            decimal? fl = null, ff = null, fs = null, sl = null;
            TroquelDescripcion? troquel = null;
            bool cintaSeg = false, cintaPd = false;
            var sinDefinir = new List<string>();

            foreach (Match grupo in Parentesis.Matches(texto))
            {
                
                var contenido = grupo.Groups[1].Value.Replace("P/D", "P_D");

                foreach (var bruto in contenido.Split('/'))
                {
                    var token = bruto.Trim();
                    if (token.Length == 0) continue;

                    var med = Medida.Match(token);
                    if (med.Success)
                    {
                        var valor = Numero(med.Groups[2].Value);
                        switch (med.Groups[1].Value)
                        {
                            case "FL": fl = valor; break;
                            case "FF": ff = valor; break;
                            case "FS": fs = valor; break;
                            case "SL": sl = valor; break;
                        }
                        continue;
                    }

                    switch (token)
                    {
                        case "OV": AsignarTroquel(TroquelDescripcion.Ovalo); break;
                        case "G": AsignarTroquel(TroquelDescripcion.Gancho); break;
                        case "ASA": AsignarTroquel(TroquelDescripcion.Asa); break;
                        case "CINTA SEGURIDAD": cintaSeg = true; break;
                        case "CINTA P_D": cintaPd = true; break;
                        default:
                            var corto = token.Length > LongitudMaxCodigo ? token[..LongitudMaxCodigo] : token;
                            if (sinDefinir.Count < MaxCodigosSinDefinir && !sinDefinir.Contains(corto))
                                sinDefinir.Add(corto);
                            break;
                    }
                }
            }

            return new DescripcionInterpretada
            {
                Descripcion = texto,
                Valida = true,
                Advertencias = advertencias,
                Material = material,
                Biodegradable = PalBiodegradable.IsMatch(texto),
                Impreso = PalImpreso.IsMatch(texto),
                FuelleLateral = fl,
                FuelleFondo = ff,
                FuelleSuperior = fs,
                Solapa = sl,
                Troquel = troquel,
                CintaSeguridad = cintaSeg,
                CintaPegaDespega = cintaPd,
                CodigosSinDefinir = sinDefinir
            };

            void AsignarTroquel(TroquelDescripcion nuevo)
            {
                if (troquel is null) troquel = nuevo;
                else if (troquel != nuevo) advertencias.Add("La descripción trae más de un troquel; se usó el primero.");
            }
        }

        private static decimal? Numero(string texto)
            => decimal.TryParse(texto.Replace(',', '.'), NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var v) ? v : null;

        private static DescripcionInterpretada Invalida(string descripcion, string error)
            => new() { Descripcion = descripcion, Valida = false, Errores = new[] { error } };
    }
}
