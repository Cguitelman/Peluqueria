using Peluqueria.Api.Models;

namespace Peluqueria.Api.Repositories;

public interface IClienteRepository
{
    Task<List<Cliente>> BuscarAsync(string? nombre, string? apellido, bool incluirInactivos);
    Task<Cliente?> ObtenerActivoPorIdAsync(int id);
    Task<Cliente?> ObtenerPorIdAsync(int id);
    Task<bool> ExisteActivoAsync(int id);
    Task<bool> ExisteEmailAsync(string email, int? excluirId = null);
    Task AgregarAsync(Cliente cliente);
    Task GuardarCambiosAsync();
}
