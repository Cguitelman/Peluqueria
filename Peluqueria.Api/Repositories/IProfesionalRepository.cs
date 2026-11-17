using Peluqueria.Api.Models;

namespace Peluqueria.Api.Repositories;

public interface IProfesionalRepository
{
    Task<List<Profesional>> BuscarAsync(string? nombre, string? apellido, string? especialidad, bool incluirInactivos);
    Task<Profesional?> ObtenerActivoPorIdAsync(int id);
    Task<Profesional?> ObtenerPorIdAsync(int id);
    Task<bool> ExisteActivoAsync(int id);
    Task<bool> ExisteEmailAsync(string email, int? excluirId = null);
    Task AgregarAsync(Profesional profesional);
    Task GuardarCambiosAsync();
}
