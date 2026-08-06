namespace SistemaFlota.Services.Notificaciones;

public interface INotificacionRecepcionService
{
    Task EnviarRecepcionMercanciaAsync(int recepcionId, List<int> usuarios);
}