using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace SistemaFlota
{
    public enum IntencionViaje { Ninguna, Salida, PuntoEntrega, Llegada, Novedad, Retraso, Ayuda }

   
    public static class InterpreteConductor
    {
        private static readonly Regex Placa = new(@"\b([a-z]{3})\s?(\d{3}|\d{2}[a-z])\b", RegexOptions.Compiled);

        private static readonly HashSet<string> Relleno = new()
        {
            "salgo", "salir", "sali", "saliendo", "salida", "voy", "vamos", "a", "al", "para", "hacia", "pa", "rumbo",
            "ruta", "la", "el", "los", "las", "de", "del", "en", "con", "y", "un", "una", "autorizacion", "nueva", "otra",
            "vuelta", "solo", "placa", "vehiculo", "carro", "camion", "moto", "arranco", "ya", "hoy", "ahora", "tipo",
            "por", "favor", "me", "toca", "tengo", "quiero", "necesito", "iniciar", "crear", "pedir", "hola", "buenos",
            "buenas", "dias", "tardes", "noches", "gracias", "pues", "bueno", "listo", "ok", "si"
        };

        public static string Normalizar(string? texto)
        {
            if (string.IsNullOrWhiteSpace(texto)) return "";
            var limpio = new StringBuilder();
            foreach (var c in texto.ToLowerInvariant().Normalize(NormalizationForm.FormD))
            {
                if (CharUnicodeInfo.GetUnicodeCategory(c) == UnicodeCategory.NonSpacingMark) continue;
                limpio.Append(char.IsLetterOrDigit(c) ? c : ' ');
            }
            return Regex.Replace(limpio.ToString(), @"\s+", " ").Trim();
        }

        
        private static bool Tiene(string t, params string[] frases) => frases.Any(f => $" {t} ".Contains($" {f} "));

        
        private static bool Empieza(string t, params string[] raices) =>
            t.Split(' ').Any(p => raices.Any(r => p.StartsWith(r)));

        public static IntencionViaje DetectarDuranteViaje(string t)
        {
            if (t == "") return IntencionViaje.Ninguna;

            if (Tiene(t, "llegare tarde", "voy tarde", "voy retrasado", "me demoro", "me voy a demorar", "voy a llegar tarde")
                || Empieza(t, "retras", "trancon", "demor"))
                return IntencionViaje.Retraso;

            if (Tiene(t, "necesito asistencia", "necesito ayuda", "requiero combustible", "sin combustible", "sin gasolina",
                         "falla mecanica", "estoy varado", "me vare")
                || Empieza(t, "asisten", "combustib", "gasolin", "acpm", "tanque", "falla", "varad", "pinch", "llanta",
                              "accident", "choq", "emergenc", "dano", "averi", "problem"))
                return IntencionViaje.Novedad;

            if (Tiene(t, "en punto de entrega", "punto de entrega", "en el cliente", "donde el cliente", "llegue al cliente",
                         "estoy entregando", "en el punto"))
                return IntencionViaje.PuntoEntrega;

            if (Tiene(t, "ruta completada", "entrega completada", "ruta terminada", "ya llegue", "llegue", "llegamos",
                         "estoy de regreso", "ya estoy en la empresa", "llegue a bodega")
                || Empieza(t, "termin", "finaliz", "complet"))
                return IntencionViaje.Llegada;

            if (Tiene(t, "saliendo de bodega", "ya sali", "sali", "saliendo", "voy saliendo", "arranco", "arranque",
                         "en camino", "en ruta", "salimos"))
                return IntencionViaje.Salida;

            if (EsAyuda(t)) return IntencionViaje.Ayuda;

            return IntencionViaje.Ninguna;
        }

        public static bool PideIniciarSalida(string t) =>
            Tiene(t, "salgo", "salir", "salida", "voy a salir", "ruta", "autorizacion", "nueva vuelta", "otra vuelta",
                     "sali", "saliendo", "arranco");

        public static bool EsAyuda(string t) => Tiene(t, "ayuda", "menu", "opciones", "comandos");

        public static bool EsCancelar(string t) => Tiene(t, "cancelar", "cancela", "cancelo", "anular");

        public static bool EsAfirmativo(string t) =>
            Tiene(t, "si", "s", "sii", "sip", "ok", "okey", "dale", "listo", "claro", "de una", "hagale", "confirmo",
                     "correcto", "afirmativo", "exacto", "eso es");

        public static bool EsNegativo(string t) => Tiene(t, "no", "nop", "negativo");

        public static string? DetectarTipoVuelta(string t)
        {
            if (Tiene(t, "mixta", "mixto")) return "Mixta";
            if (Empieza(t, "mensaj", "diligenc")) return "Mensajería";

            var entrega = Empieza(t, "entreg");
            var recoge = Empieza(t, "recog", "recoj");
            if (entrega && recoge) return "Recoge y Entrega";
            if (recoge) return "Solo Recoge";
            if (entrega) return "Solo Entrega";
            return null;
        }

        public static string? DetectarPlaca(string t)
        {
            var m = Placa.Match(t);
            return m.Success ? (m.Groups[1].Value + m.Groups[2].Value).ToUpperInvariant() : null;
        }

        
        public static string? DetectarDestino(string t)
        {
            var hayPista = Tiene(t, "para", "hacia", "pa", "rumbo") || DetectarTipoVuelta(t) != null || DetectarPlaca(t) != null;
            if (!hayPista) return null;

            var sinPlaca = Placa.Replace(t, " ");
            var palabras = sinPlaca.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Where(p => !Relleno.Contains(p))
                .Where(p => !p.StartsWith("entreg") && !p.StartsWith("recog") && !p.StartsWith("recoj")
                            && !p.StartsWith("mensaj") && !p.StartsWith("mixt"))
                .ToList();

            if (palabras.Count == 0) return null;
            var destino = string.Join(' ', palabras);
            return destino.Length > 100 ? null : CultureInfo.GetCultureInfo("es-CO").TextInfo.ToTitleCase(destino);
        }
    }
}
namespace SistemaFlota.Services
{
    public class InterpreteConductor
    {
    }
}
