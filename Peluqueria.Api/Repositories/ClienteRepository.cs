using Microsoft.EntityFrameworkCore;
using Peluqueria.Api.Data;
using Peluqueria.Api.Models;

namespace Peluqueria.Api.Repositories;

public class ClienteRepository : IClienteRepository
{
    private readonly ApplicationDbContext _db;
    public ClienteRepository(ApplicationDbContext db) => _db = db;

    public async Task<List<Cliente>> BuscarAsync(string? nombre, string? apellido, bool incluirInactivos)
    {
        var query = _db.Clientes.AsNoTracking().AsQueryable();

        if (!incluirInactivos)
            query = query.Where(c => c.Activo);

        if (!string.IsNullOrWhiteSpace(nombre))
        {
            var texto = nombre.Trim();
            query = query.Where(c => c.Nombre.Contains(texto));
        }

        if (!string.IsNullOrWhiteSpace(apellido))
        {
            var texto = apellido.Trim();
            query = query.Where(c => c.Apellido.Contains(texto));
        }

        return await query
            .OrderByDescending(c => c.Activo)
            .ThenBy(c => c.Apellido)
            .ThenBy(c => c.Nombre)
            .ToListAsync();
    }

    public Task<Cliente?> ObtenerActivoPorIdAsync(int id) =>
        _db.Clientes.FirstOrDefaultAsync(c => c.Id == id && c.Activo);

    public Task<Cliente?> ObtenerPorIdAsync(int id) =>
        _db.Clientes.FirstOrDefaultAsync(c => c.Id == id);

    public Task<bool> ExisteActivoAsync(int id) =>
        _db.Clientes.AnyAsync(c => c.Id == id && c.Activo);

    public Task<bool> ExisteEmailAsync(string email, int? excluirId = null)
    {
        var normalizado = email.Trim().ToLower();
        return _db.Clientes.AnyAsync(c =>
            c.Email.ToLower() == normalizado &&
            (!excluirId.HasValue || c.Id != excluirId.Value));
    }

    public async Task AgregarAsync(Cliente cliente) => await _db.Clientes.AddAsync(cliente);

    public Task GuardarCambiosAsync() => _db.SaveChangesAsync();
}
