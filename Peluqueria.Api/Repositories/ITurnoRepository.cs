using Peluqueria.Api.Models;

namespace Peluqueria.Api.Repositories;

public interface ITurnoRepository
{
    Task<List<Turno>> BuscarAsync(
        DateTime? fecha,
        DateTime? fechaDesde,
        DateTime? fechaHasta,
        int? profesionalId,
        EstadoTurno? estado,
        string? cliente,
        string? profesional,
        string? servicio,
        bool incluirInactivos);
    Task<Turno?> ObtenerActivoPorIdAsync(int id, bool conRelaciones = false);
    Task<Turno?> ObtenerPorIdAsync(int id, bool conRelaciones = false);
    Task<bool> ExisteConflictoAsync(int profesionalId, DateTime fechaHora, decimal duracionHoras, int? turnoId);
    Task<bool> TieneTurnosFuturosActivosClienteAsync(int clienteId);
    Task<bool> TieneTurnosFuturosActivosProfesionalAsync(int profesionalId);
    Task<bool> TieneTurnosFuturosActivosServicioAsync(int servicioId);
    Task AgregarAsync(Turno turno);
    Task GuardarCambiosAsync();
}
