namespace Peluqueria.Api.Services;

public class NotificadorConsola : INotificador
{
    public Task NotificarAsync(string mensaje)
    {
        Console.WriteLine($"[NOTIFICACION] {mensaje}");
        return Task.CompletedTask;
    }
}
