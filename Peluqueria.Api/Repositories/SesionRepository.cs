using Microsoft.EntityFrameworkCore;
using Peluqueria.Api.Data;
using Peluqueria.Api.Models;

namespace Peluqueria.Api.Repositories;

public class SesionRepository : ISesionRepository
{
    private readonly ApplicationDbContext _db;
    public SesionRepository(ApplicationDbContext db) => _db = db;

    public async Task AgregarAsync(SesionUsuario sesion) => await _db.SesionesUsuario.AddAsync(sesion);

    public Task<SesionUsuario?> ObtenerValidaPorHashAsync(string tokenHash) =>
        _db.SesionesUsuario.AsNoTracking()
            .Include(s => s.Usuario)
            .FirstOrDefaultAsync(s =>
                s.TokenHash == tokenHash &&
                s.Activa &&
                s.FechaExpiracion > DateTime.UtcNow &&
                s.Usuario.Activo);

    public async Task RevocarAsync(string tokenHash)
    {
        var sesion = await _db.SesionesUsuario.FirstOrDefaultAsync(s => s.TokenHash == tokenHash && s.Activa);
        if (sesion is not null)
            sesion.Activa = false;
    }

    public async Task LimpiarVencidasAsync()
    {
        var vencidas = await _db.SesionesUsuario
            .Where(s => !s.Activa || s.FechaExpiracion <= DateTime.UtcNow)
            .ToListAsync();

        if (vencidas.Count > 0)
            _db.SesionesUsuario.RemoveRange(vencidas);
    }

    public Task GuardarCambiosAsync() => _db.SaveChangesAsync();
}
