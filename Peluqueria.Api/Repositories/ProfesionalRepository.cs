using Microsoft.EntityFrameworkCore;
using Peluqueria.Api.Data;
using Peluqueria.Api.Models;

namespace Peluqueria.Api.Repositories;

public class ProfesionalRepository : IProfesionalRepository
{
    private readonly ApplicationDbContext _db;
    public ProfesionalRepository(ApplicationDbContext db) => _db = db;

    public async Task<List<Profesional>> BuscarAsync(string? nombre, string? apellido, string? especialidad, bool incluirInactivos)
    {
        var query = _db.Profesionales.AsNoTracking().AsQueryable();

        if (!incluirInactivos)
            query = query.Where(p => p.Activo);

        if (!string.IsNullOrWhiteSpace(nombre))
        {
            var texto = nombre.Trim();
            query = query.Where(p => p.Nombre.Contains(texto));
        }

        if (!string.IsNullOrWhiteSpace(apellido))
        {
            var texto = apellido.Trim();
            query = query.Where(p => p.Apellido.Contains(texto));
        }

        if (!string.IsNullOrWhiteSpace(especialidad))
        {
            var texto = especialidad.Trim();
            query = query.Where(p => p.Especialidad.Contains(texto));
        }

        return await query
            .OrderByDescending(p => p.Activo)
            .ThenBy(p => p.Apellido)
            .ThenBy(p => p.Nombre)
            .ToListAsync();
    }

    public Task<Profesional?> ObtenerActivoPorIdAsync(int id) =>
        _db.Profesionales.FirstOrDefaultAsync(p => p.Id == id && p.Activo);

    public Task<Profesional?> ObtenerPorIdAsync(int id) =>
        _db.Profesionales.FirstOrDefaultAsync(p => p.Id == id);

    public Task<bool> ExisteActivoAsync(int id) =>
        _db.Profesionales.AnyAsync(p => p.Id == id && p.Activo);

    public Task<bool> ExisteEmailAsync(string email, int? excluirId = null)
    {
        var normalizado = email.Trim().ToLower();
        return _db.Profesionales.AnyAsync(p =>
            p.Email.ToLower() == normalizado &&
            (!excluirId.HasValue || p.Id != excluirId.Value));
    }

    public async Task AgregarAsync(Profesional profesional) => await _db.Profesionales.AddAsync(profesional);

    public Task GuardarCambiosAsync() => _db.SaveChangesAsync();
}
