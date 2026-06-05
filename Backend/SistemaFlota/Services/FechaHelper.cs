namespace SistemaFlota
{
    public static class FechaHelper
    {
        private static readonly TimeZoneInfo ZonaColombia =
            TimeZoneInfo.FindSystemTimeZoneById("America/Bogota");

        public static DateTime Ahora() =>
            TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, ZonaColombia);
    }
}