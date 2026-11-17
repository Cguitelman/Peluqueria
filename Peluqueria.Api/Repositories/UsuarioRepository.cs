using Microsoft.EntityFrameworkCore;
using Peluqueria.Api.Data;
using Peluqueria.Api.Models;

namespace Peluqueria.Api.Repositories;

public class UsuarioRepository : IUsuarioRepository
{
    private readonly ApplicationDbContext _db;
    public UsuarioRepository(ApplicationDbContext db) => _db = db;

    public Task<Usuario?> ObtenerActivoPorNombreAsync(string nombreUsuario) =>
        _db.Usuarios.AsNoTracking().FirstOrDefaultAsync(u => u.NombreUsuario == nombreUsuario && u.Activo);

    public Task<Usuario?> ObtenerActivoPorIdAsync(int id) =>
        _db.Usuarios.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id && u.Activo);
}
