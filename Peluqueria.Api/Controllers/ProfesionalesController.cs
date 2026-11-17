using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Peluqueria.Api.DTOs;
using Peluqueria.Api.Models;
using Peluqueria.Api.Repositories;
using Peluqueria.Api.Security;

namespace Peluqueria.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = Roles.Personal)]
public class ProfesionalesController : ControllerBase
{
    private readonly IProfesionalRepository _profesionales;
    private readonly ITurnoRepository _turnos;

    public ProfesionalesController(IProfesionalRepository profesionales, ITurnoRepository turnos)
    {
        _profesionales = profesionales;
        _turnos = turnos;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProfesionalDto>>> GetTodos(
        [FromQuery] string? nombre,
        [FromQuery] string? apellido,
        [FromQuery] string? especialidad,
        [FromQuery] bool incluirInactivos = false)
    {
        var items = await _profesionales.BuscarAsync(nombre, apellido, especialidad, incluirInactivos);
        return Ok(items.Select(Map));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProfesionalDto>> GetPorId(int id)
    {
        var profesional = await _profesionales.ObtenerActivoPorIdAsync(id);
        return profesional is null ? NotFound() : Ok(Map(profesional));
    }

    [HttpPost]
    [Authorize(Roles = Roles.Administrador)]
    public async Task<ActionResult<ProfesionalDto>> Crear(GuardarProfesionalDto dto)
    {
        var email = dto.Email.Trim();
        if (await _profesionales.ExisteEmailAsync(email))
            return BadRequest(new { mensaje = "Ya existe un profesional registrado con ese email." });

        var profesional = new Profesional
        {
            Nombre = dto.Nombre.Trim(),
            Apellido = dto.Apellido.Trim(),
            Email = email,
            Especialidad = dto.Especialidad.Trim(),
            Activo = true
        };

        await _profesionales.AgregarAsync(profesional);
        await _profesionales.GuardarCambiosAsync();
        return CreatedAtAction(nameof(GetPorId), new { id = profesional.Id }, Map(profesional));
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = Roles.Administrador)]
    public async Task<IActionResult> Actualizar(int id, GuardarProfesionalDto dto)
    {
        var profesional = await _profesionales.ObtenerActivoPorIdAsync(id);
        if (profesional is null) return NotFound();

        var email = dto.Email.Trim();
        if (await _profesionales.ExisteEmailAsync(email, id))
            return BadRequest(new { mensaje = "Ya existe otro profesional registrado con ese email." });

        profesional.Nombre = dto.Nombre.Trim();
        profesional.Apellido = dto.Apellido.Trim();
        profesional.Email = email;
        profesional.Especialidad = dto.Especialidad.Trim();
        await _profesionales.GuardarCambiosAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = Roles.Administrador)]
    public async Task<IActionResult> Eliminar(int id)
    {
        var profesional = await _profesionales.ObtenerActivoPorIdAsync(id);
        if (profesional is null) return NotFound();

        if (await _turnos.TieneTurnosFuturosActivosProfesionalAsync(id))
            return BadRequest(new { mensaje = "No se puede desactivar al profesional porque tiene turnos futuros activos." });

        profesional.Activo = false;
        await _profesionales.GuardarCambiosAsync();
        return NoContent();
    }

    [HttpPost("{id:int}/activar")]
    [Authorize(Roles = Roles.Administrador)]
    public async Task<IActionResult> Activar(int id)
    {
        var profesional = await _profesionales.ObtenerPorIdAsync(id);
        if (profesional is null) return NotFound();
        if (profesional.Activo) return NoContent();

        profesional.Activo = true;
        await _profesionales.GuardarCambiosAsync();
        return NoContent();
    }

    private static ProfesionalDto Map(Profesional p) => new(p.Id, p.Nombre, p.Apellido, p.Email, p.Especialidad, p.Activo);
}
