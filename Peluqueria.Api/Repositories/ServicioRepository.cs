using Microsoft.EntityFrameworkCore;
using Peluqueria.Api.Data;
using Peluqueria.Api.Models;

namespace Peluqueria.Api.Repositories;

public class ServicioRepository : IServicioRepository
{
    private readonly ApplicationDbContext _db;
    public ServicioRepository(ApplicationDbContext db) => _db = db;

    public async Task<List<Servicio>> BuscarAsync(string? nombre, bool incluirInactivos)
    {
        var query = _db.Servicios.AsNoTracking().AsQueryable();

        if (!incluirInactivos)
            query = query.Where(s => s.Activo);

        if (!string.IsNullOrWhiteSpace(nombre))
        {
            var texto = nombre.Trim();
            query = query.Where(s => s.Nombre.Contains(texto));
        }

        return await query
            .OrderByDescending(s => s.Activo)
            .ThenBy(s => s.Nombre)
            .ToListAsync();
    }

    public Task<Servicio?> ObtenerActivoPorIdAsync(int id) =>
        _db.Servicios.FirstOrDefaultAsync(s => s.Id == id && s.Activo);

    public Task<Servicio?> ObtenerPorIdAsync(int id) =>
        _db.Servicios.FirstOrDefaultAsync(s => s.Id == id);

    public Task<bool> ExisteActivoAsync(int id) =>
        _db.Servicios.AnyAsync(s => s.Id == id && s.Activo);

    public async Task AgregarAsync(Servicio servicio) => await _db.Servicios.AddAsync(servicio);

    public Task GuardarCambiosAsync() => _db.SaveChangesAsync();
}
