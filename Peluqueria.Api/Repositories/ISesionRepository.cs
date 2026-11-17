using Peluqueria.Api.Models;

namespace Peluqueria.Api.Repositories;

public interface ISesionRepository
{
    Task AgregarAsync(SesionUsuario sesion);
    Task<SesionUsuario?> ObtenerValidaPorHashAsync(string tokenHash);
    Task RevocarAsync(string tokenHash);
    Task LimpiarVencidasAsync();
    Task GuardarCambiosAsync();
}
