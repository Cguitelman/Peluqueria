using Microsoft.EntityFrameworkCore;
using Peluqueria.Api.Data;
using Peluqueria.Api.Models;

namespace Peluqueria.Api.Repositories;

public class TurnoRepository : ITurnoRepository
{
    private readonly ApplicationDbContext _db;
    public TurnoRepository(ApplicationDbContext db) => _db = db;

    public async Task<List<Turno>> BuscarAsync(
        DateTime? fecha,
        DateTime? fechaDesde,
        DateTime? fechaHasta,
        int? profesionalId,
        EstadoTurno? estado,
        string? cliente,
        string? profesional,
        string? servicio,
        bool incluirInactivos)
    {
        var query = _db.Turnos.AsNoTracking()
            .Include(t => t.Cliente)
            .Include(t => t.Profesional)
            .Include(t => t.Servicio)
            .AsQueryable();

        if (!incluirInactivos)
            query = query.Where(t => t.Activo);

        if (fecha.HasValue)
        {
            var inicio = fecha.Value.Date;
            var fin = inicio.AddDays(1);
            query = query.Where(t => t.FechaHora >= inicio && t.FechaHora < fin);
        }

        if (fechaDesde.HasValue)
            query = query.Where(t => t.FechaHora >= fechaDesde.Value.Date);

        if (fechaHasta.HasValue)
        {
            var fin = fechaHasta.Value.Date.AddDays(1);
            query = query.Where(t => t.FechaHora < fin);
        }

        if (profesionalId.HasValue)
            query = query.Where(t => t.ProfesionalId == profesionalId.Value);

        if (estado.HasValue)
            query = query.Where(t => t.Estado == estado.Value);

        if (!string.IsNullOrWhiteSpace(cliente))
        {
            var texto = cliente.Trim();
            query = query.Where(t => t.Cliente.Nombre.Contains(texto) || t.Cliente.Apellido.Contains(texto));
        }

        if (!string.IsNullOrWhiteSpace(profesional))
        {
            var texto = profesional.Trim();
            query = query.Where(t => t.Profesional.Nombre.Contains(texto) || t.Profesional.Apellido.Contains(texto));
        }

        if (!string.IsNullOrWhiteSpace(servicio))
        {
            var texto = servicio.Trim();
            query = query.Where(t => t.Servicio.Nombre.Contains(texto));
        }

        return await query
            .OrderByDescending(t => t.Activo)
            .ThenBy(t => t.FechaHora)
            .ToListAsync();
    }

    public Task<Turno?> ObtenerActivoPorIdAsync(int id, bool conRelaciones = false) =>
        ObtenerPorIdInternoAsync(id, soloActivos: true, conRelaciones);

    public Task<Turno?> ObtenerPorIdAsync(int id, bool conRelaciones = false) =>
        ObtenerPorIdInternoAsync(id, soloActivos: false, conRelaciones);

    private async Task<Turno?> ObtenerPorIdInternoAsync(int id, bool soloActivos, bool conRelaciones)
    {
        IQueryable<Turno> query = _db.Turnos.Where(t => t.Id == id);
        if (soloActivos)
            query = query.Where(t => t.Activo);

        if (conRelaciones)
        {
            query = query.Include(t => t.Cliente)
                .Include(t => t.Profesional)
                .Include(t => t.Servicio);
        }

        return await query.FirstOrDefaultAsync();
    }

    public async Task<bool> ExisteConflictoAsync(int profesionalId, DateTime fechaHora, decimal duracionHoras, int? turnoId)
    {
        var inicioDia = fechaHora.Date;
        var finDia = inicioDia.AddDays(1);

        var turnosDelProfesional = await _db.Turnos.AsNoTracking()
            .Include(t => t.Servicio)
            .Where(t =>
                t.Activo &&
                t.ProfesionalId == profesionalId &&
                t.Estado != EstadoTurno.Cancelado &&
                t.FechaHora >= inicioDia && t.FechaHora < finDia &&
                (!turnoId.HasValue || t.Id != turnoId.Value))
            .ToListAsync();

        var finNuevo = fechaHora.AddMinutes((double)(duracionHoras * 60m));

        return turnosDelProfesional.Any(t =>
        {
            var finExistente = t.FechaHora.AddMinutes((double)(t.Servicio.DuracionHoras * 60m));
            return t.FechaHora < finNuevo && fechaHora < finExistente;
        });
    }

    public Task<bool> TieneTurnosFuturosActivosClienteAsync(int clienteId) =>
        _db.Turnos.AnyAsync(t =>
            t.Activo && t.ClienteId == clienteId && t.Estado != EstadoTurno.Cancelado && t.FechaHora > DateTime.Now);

    public Task<bool> TieneTurnosFuturosActivosProfesionalAsync(int profesionalId) =>
        _db.Turnos.AnyAsync(t =>
            t.Activo && t.ProfesionalId == profesionalId && t.Estado != EstadoTurno.Cancelado && t.FechaHora > DateTime.Now);

    public Task<bool> TieneTurnosFuturosActivosServicioAsync(int servicioId) =>
        _db.Turnos.AnyAsync(t =>
            t.Activo && t.ServicioId == servicioId && t.Estado != EstadoTurno.Cancelado && t.FechaHora > DateTime.Now);

    public async Task AgregarAsync(Turno turno) => await _db.Turnos.AddAsync(turno);

    public Task GuardarCambiosAsync() => _db.SaveChangesAsync();
}
