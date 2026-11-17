using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Peluqueria.Api.Configuracion;
using Peluqueria.Api.DTOs;
using Peluqueria.Api.Models;
using Peluqueria.Api.Repositories;
using Peluqueria.Api.Security;
using Peluqueria.Api.Services;

namespace Peluqueria.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = Roles.Personal)]
public class TurnosController : ControllerBase
{
    private readonly ITurnoRepository _turnos;
    private readonly IClienteRepository _clientes;
    private readonly IProfesionalRepository _profesionales;
    private readonly IServicioRepository _servicios;
    private readonly INotificador _notificador;
    private readonly AgendaOptions _agenda;

    public TurnosController(
        ITurnoRepository turnos,
        IClienteRepository clientes,
        IProfesionalRepository profesionales,
        IServicioRepository servicios,
        INotificador notificador,
        IOptions<AgendaOptions> agenda)
    {
        _turnos = turnos;
        _clientes = clientes;
        _profesionales = profesionales;
        _servicios = servicios;
        _notificador = notificador;
        _agenda = agenda.Value;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<TurnoDto>>> GetTodos(
        [FromQuery] DateTime? fecha,
        [FromQuery] DateTime? fechaDesde,
        [FromQuery] DateTime? fechaHasta,
        [FromQuery] int? profesionalId,
        [FromQuery] EstadoTurno? estado,
        [FromQuery] string? cliente,
        [FromQuery] string? profesional,
        [FromQuery] string? servicio,
        [FromQuery] bool incluirInactivos = false)
    {
        if (fechaDesde.HasValue && fechaHasta.HasValue && fechaHasta.Value.Date < fechaDesde.Value.Date)
            return BadRequest(new { mensaje = "La fecha hasta no puede ser anterior a la fecha desde." });

        var items = await _turnos.BuscarAsync(
            fecha, fechaDesde, fechaHasta, profesionalId, estado, cliente, profesional, servicio, incluirInactivos);
        return Ok(items.Select(Map));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<TurnoDto>> GetPorId(int id)
    {
        var turno = await _turnos.ObtenerActivoPorIdAsync(id, conRelaciones: true);
        return turno is null ? NotFound() : Ok(Map(turno));
    }

    [HttpPost]
    public async Task<ActionResult<TurnoDto>> Crear(CrearTurnoDto dto)
    {
        var error = await ValidarTurno(
            dto.ClienteId, dto.ProfesionalId, dto.ServicioId, dto.FechaHora, null, exigirFechaFutura: true);
        if (error is not null) return BadRequest(new { mensaje = error });

        var turno = new Turno
        {
            ClienteId = dto.ClienteId,
            ProfesionalId = dto.ProfesionalId,
            ServicioId = dto.ServicioId,
            FechaHora = dto.FechaHora,
            Activo = true
        };

        await _turnos.AgregarAsync(turno);
        await _turnos.GuardarCambiosAsync();
        await _notificador.NotificarAsync($"Nuevo turno #{turno.Id} creado para {turno.FechaHora:g}.");

        var creado = await _turnos.ObtenerActivoPorIdAsync(turno.Id, conRelaciones: true);
        return CreatedAtAction(nameof(GetPorId), new { id = turno.Id }, Map(creado!));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Actualizar(int id, ActualizarTurnoDto dto)
    {
        var turno = await _turnos.ObtenerActivoPorIdAsync(id);
        if (turno is null) return NotFound();

        var cambiaAsignacionOFecha =
            turno.ClienteId != dto.ClienteId ||
            turno.ProfesionalId != dto.ProfesionalId ||
            turno.ServicioId != dto.ServicioId ||
            turno.FechaHora != dto.FechaHora;

        var esTurnoPasado = dto.FechaHora <= DateTime.Now;
        var vuelveAAgenda = turno.Estado == EstadoTurno.Cancelado && dto.Estado != EstadoTurno.Cancelado;

        // Un turno pasado puede cerrarse como Finalizado o Cancelado, pero no volver a Pendiente/Confirmado.
        if (!cambiaAsignacionOFecha && esTurnoPasado && dto.Estado is EstadoTurno.Pendiente or EstadoTurno.Confirmado)
            return BadRequest(new { mensaje = "Un turno pasado sólo puede quedar Finalizado o Cancelado." });

        // Reprogramar siempre exige fecha futura. Si un turno cancelado vuelve a la agenda, también se revalida.
        if (cambiaAsignacionOFecha || vuelveAAgenda)
        {
            var error = await ValidarTurno(
                dto.ClienteId, dto.ProfesionalId, dto.ServicioId, dto.FechaHora, id, exigirFechaFutura: cambiaAsignacionOFecha || vuelveAAgenda);
            if (error is not null) return BadRequest(new { mensaje = error });
        }

        turno.ClienteId = dto.ClienteId;
        turno.ProfesionalId = dto.ProfesionalId;
        turno.ServicioId = dto.ServicioId;
        turno.FechaHora = dto.FechaHora;
        turno.CambiarEstado(dto.Estado);

        await _turnos.GuardarCambiosAsync();
        await _notificador.NotificarAsync($"Turno #{id} actualizado. Estado: {dto.Estado}.");
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Eliminar(int id)
    {
        var turno = await _turnos.ObtenerActivoPorIdAsync(id);
        if (turno is null) return NotFound();

        turno.Activo = false;
        await _turnos.GuardarCambiosAsync();
        return NoContent();
    }

    [HttpPost("{id:int}/activar")]
    public async Task<IActionResult> Activar(int id)
    {
        var turno = await _turnos.ObtenerPorIdAsync(id);
        if (turno is null) return NotFound();
        if (turno.Activo) return NoContent();

        var error = await ValidarTurno(
            turno.ClienteId, turno.ProfesionalId, turno.ServicioId, turno.FechaHora, turno.Id, exigirFechaFutura: true);
        if (error is not null) return BadRequest(new { mensaje = $"No se puede reactivar el turno. {error}" });

        turno.Activo = true;
        await _turnos.GuardarCambiosAsync();
        return NoContent();
    }

    private async Task<string?> ValidarTurno(
        int clienteId,
        int profesionalId,
        int servicioId,
        DateTime fechaHora,
        int? turnoId,
        bool exigirFechaFutura)
    {
        if (exigirFechaFutura && fechaHora <= DateTime.Now)
            return "La fecha y hora del turno deben ser posteriores al momento actual.";

        if (_agenda.IntervaloMinutos <= 0)
            _agenda.IntervaloMinutos = 30;

        if (fechaHora.Second != 0 || fechaHora.Millisecond != 0 || fechaHora.Minute % _agenda.IntervaloMinutos != 0)
            return $"Los turnos deben comenzar en intervalos de {_agenda.IntervaloMinutos} minutos.";

        var apertura = TimeSpan.FromHours(_agenda.HoraApertura);
        var cierre = TimeSpan.FromHours(_agenda.HoraCierre);
        if (fechaHora.TimeOfDay < apertura || fechaHora.TimeOfDay >= cierre)
            return $"El horario de atención es de {_agenda.HoraApertura:00}:00 a {_agenda.HoraCierre:00}:00.";

        if (!await _clientes.ExisteActivoAsync(clienteId))
            return "El cliente indicado no existe o está desactivado.";

        if (!await _profesionales.ExisteActivoAsync(profesionalId))
            return "El profesional indicado no existe o está desactivado.";

        var servicio = await _servicios.ObtenerActivoPorIdAsync(servicioId);
        if (servicio is null)
            return "El servicio indicado no existe o está desactivado.";

        var finTurno = fechaHora.AddMinutes((double)(servicio.DuracionHoras * 60m));
        if (finTurno.Date != fechaHora.Date || finTurno.TimeOfDay > cierre)
            return $"El servicio debe finalizar antes de las {_agenda.HoraCierre:00}:00.";

        if (await _turnos.ExisteConflictoAsync(profesionalId, fechaHora, servicio.DuracionHoras, turnoId))
            return "El profesional ya tiene otro turno que se superpone con ese horario.";

        return null;
    }

    private static TurnoDto Map(Turno t) => new(
        t.Id, t.FechaHora, t.Estado,
        t.ClienteId, $"{t.Cliente.Nombre} {t.Cliente.Apellido}",
        t.ProfesionalId, $"{t.Profesional.Nombre} {t.Profesional.Apellido}",
        t.ServicioId, t.Servicio.Nombre, t.Servicio.DuracionHoras, t.Servicio.Precio, t.Activo);
}
