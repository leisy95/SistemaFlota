namespace SistemaFlota.Services.Helper.InventariosBodega
{
    public class ReferenciaHelper
    {
        public static string Normalize(string referencia)
        {
            if (string.IsNullOrWhiteSpace(referencia))
                return string.Empty;

            return referencia
                .Replace(".", "")
                .Replace("*", "")
                .Trim()
                .ToUpper();
        }
    }
}