namespace SistemaFlota
{
    public interface IMensajeriaService
    {
        Task EnviarMensajeAsync(string numeroDestino, string mensaje);
        Task EnviarMensajeAsync(string numeroDestino, string mensaje, string categoria);
        Task EnviarAMultiplesAsync(List<string> numeros, string mensaje);
        Task EnviarAMultiplesAsync(List<string> numeros, string mensaje, string categoria);
    }
}