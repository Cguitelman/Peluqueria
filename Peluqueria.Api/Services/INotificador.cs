namespace Peluqueria.Api.Services;

public interface INotificador
{
    Task NotificarAsync(string mensaje);
}
