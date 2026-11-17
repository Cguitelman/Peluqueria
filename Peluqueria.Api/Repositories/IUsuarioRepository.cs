using Peluqueria.Api.Models;

namespace Peluqueria.Api.Repositories;

public interface IUsuarioRepository
{
    Task<Usuario?> ObtenerActivoPorNombreAsync(string nombreUsuario);
    Task<Usuario?> ObtenerActivoPorIdAsync(int id);
}
