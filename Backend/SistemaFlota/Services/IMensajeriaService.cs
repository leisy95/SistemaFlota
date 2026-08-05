namespace SistemaFlota
{
    public interface IMensajeriaService
    {
        Task EnviarMensajeAsync(string numeroDestino, string mensaje);
        Task EnviarAMultiplesAsync(List<string> numeros, string mensaje);
        Task EnviarAMultiplesAsync(List<string> numeros, string mensaje, string categoria);
    }
}