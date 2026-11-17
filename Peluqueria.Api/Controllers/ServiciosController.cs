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
public class ServiciosController : ControllerBase
{
    private readonly IServicioRepository _servicios;
    private readonly ITurnoRepository _turnos;

    public ServiciosController(IServicioRepository servicios, ITurnoRepository turnos)
    {
        _servicios = servicios;
        _turnos = turnos;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ServicioDto>>> GetTodos(
        [FromQuery] string? nombre,
        [FromQuery] bool incluirInactivos = false)
    {
        var items = await _servicios.BuscarAsync(nombre, incluirInactivos);
        return Ok(items.Select(Map));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ServicioDto>> GetPorId(int id)
    {
        var servicio = await _servicios.ObtenerActivoPorIdAsync(id);
        return servicio is null ? NotFound() : Ok(Map(servicio));
    }

    [HttpPost]
    [Authorize(Roles = Roles.Administrador)]
    public async Task<ActionResult<ServicioDto>> Crear(GuardarServicioDto dto)
    {
        if (!EsDuracionValida(dto.DuracionHoras))
            return BadRequest(new { mensaje = "La duración debe expresarse en intervalos de media hora (0.5, 1, 1.5, etc.)." });

        var servicio = new Servicio
        {
            Nombre = dto.Nombre.Trim(),
            Descripcion = dto.Descripcion.Trim(),
            DuracionHoras = dto.DuracionHoras,
            Precio = dto.Precio,
            Activo = true
        };

        await _servicios.AgregarAsync(servicio);
        await _servicios.GuardarCambiosAsync();
        return CreatedAtAction(nameof(GetPorId), new { id = servicio.Id }, Map(servicio));
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = Roles.Administrador)]
    public async Task<IActionResult> Actualizar(int id, GuardarServicioDto dto)
    {
        if (!EsDuracionValida(dto.DuracionHoras))
            return BadRequest(new { mensaje = "La duración debe expresarse en intervalos de media hora (0.5, 1, 1.5, etc.)." });

        var servicio = await _servicios.ObtenerActivoPorIdAsync(id);
        if (servicio is null) return NotFound();

        servicio.Nombre = dto.Nombre.Trim();
        servicio.Descripcion = dto.Descripcion.Trim();
        servicio.DuracionHoras = dto.DuracionHoras;
        servicio.Precio = dto.Precio;
        await _servicios.GuardarCambiosAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = Roles.Administrador)]
    public async Task<IActionResult> Eliminar(int id)
    {
        var servicio = await _servicios.ObtenerActivoPorIdAsync(id);
        if (servicio is null) return NotFound();

        if (await _turnos.TieneTurnosFuturosActivosServicioAsync(id))
            return BadRequest(new { mensaje = "No se puede desactivar el servicio porque tiene turnos futuros activos." });

        servicio.Activo = false;
        await _servicios.GuardarCambiosAsync();
        return NoContent();
    }

    [HttpPost("{id:int}/activar")]
    [Authorize(Roles = Roles.Administrador)]
    public async Task<IActionResult> Activar(int id)
    {
        var servicio = await _servicios.ObtenerPorIdAsync(id);
        if (servicio is null) return NotFound();
        if (servicio.Activo) return NoContent();

        servicio.Activo = true;
        await _servicios.GuardarCambiosAsync();
        return NoContent();
    }

    private static bool EsDuracionValida(decimal horas)
    {
        if (horas < 0.5m || horas > 10m) return false;
        return horas * 2m == decimal.Truncate(horas * 2m);
    }

    private static ServicioDto Map(Servicio s) => new(s.Id, s.Nombre, s.Descripcion, s.DuracionHoras, s.Precio, s.Activo);
}
