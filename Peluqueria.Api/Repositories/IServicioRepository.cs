using Peluqueria.Api.Models;

namespace Peluqueria.Api.Repositories;

public interface IServicioRepository
{
    Task<List<Servicio>> BuscarAsync(string? nombre, bool incluirInactivos);
    Task<Servicio?> ObtenerActivoPorIdAsync(int id);
    Task<Servicio?> ObtenerPorIdAsync(int id);
    Task<bool> ExisteActivoAsync(int id);
    Task AgregarAsync(Servicio servicio);
    Task GuardarCambiosAsync();
}
