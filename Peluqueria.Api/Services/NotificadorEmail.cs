namespace Peluqueria.Api.Services;

// Implementación simulada para demostrar que la aplicación puede cambiar
// el modo de notificar sin modificar el controlador.
public class NotificadorEmail : INotificador
{
    public async Task NotificarAsync(string mensaje)
    {
        await Task.Delay(50);
        Console.WriteLine($"[EMAIL SIMULADO] {mensaje}");
    }
}
