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
public class ClientesController : ControllerBase
{
    private readonly IClienteRepository _clientes;
    private readonly ITurnoRepository _turnos;

    public ClientesController(IClienteRepository clientes, ITurnoRepository turnos)
    {
        _clientes = clientes;
        _turnos = turnos;
    }

    /// <summary>Obtiene clientes y permite filtrar por nombre y apellido.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ClienteDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ClienteDto>>> GetTodos(
        [FromQuery] string? nombre,
        [FromQuery] string? apellido,
        [FromQuery] bool incluirInactivos = false)
    {
        var items = await _clientes.BuscarAsync(nombre, apellido, incluirInactivos);
        return Ok(items.Select(Map));
    }

    /// <summary>Obtiene un cliente activo por su identificador.</summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<ClienteDto>> GetPorId(int id)
    {
        var cliente = await _clientes.ObtenerActivoPorIdAsync(id);
        return cliente is null ? NotFound() : Ok(Map(cliente));
    }

    /// <summary>Crea un cliente.</summary>
    [HttpPost]
    public async Task<ActionResult<ClienteDto>> Crear(GuardarClienteDto dto)
    {
        var email = dto.Email.Trim();
        if (await _clientes.ExisteEmailAsync(email))
            return BadRequest(new { mensaje = "Ya existe un cliente registrado con ese email." });

        var cliente = new Cliente
        {
            Nombre = dto.Nombre.Trim(),
            Apellido = dto.Apellido.Trim(),
            Email = email,
            Telefono = dto.Telefono.Trim(),
            Activo = true
        };

        await _clientes.AgregarAsync(cliente);
        await _clientes.GuardarCambiosAsync();
        return CreatedAtAction(nameof(GetPorId), new { id = cliente.Id }, Map(cliente));
    }

    /// <summary>Actualiza un cliente activo.</summary>
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Actualizar(int id, GuardarClienteDto dto)
    {
        var cliente = await _clientes.ObtenerActivoPorIdAsync(id);
        if (cliente is null) return NotFound();

        var email = dto.Email.Trim();
        if (await _clientes.ExisteEmailAsync(email, id))
            return BadRequest(new { mensaje = "Ya existe otro cliente registrado con ese email." });

        cliente.Nombre = dto.Nombre.Trim();
        cliente.Apellido = dto.Apellido.Trim();
        cliente.Email = email;
        cliente.Telefono = dto.Telefono.Trim();
        await _clientes.GuardarCambiosAsync();
        return NoContent();
    }

    /// <summary>Desactiva un cliente mediante borrado lógico si no posee turnos futuros activos.</summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Eliminar(int id)
    {
        var cliente = await _clientes.ObtenerActivoPorIdAsync(id);
        if (cliente is null) return NotFound();

        if (await _turnos.TieneTurnosFuturosActivosClienteAsync(id))
            return BadRequest(new { mensaje = "No se puede desactivar al cliente porque tiene turnos futuros activos." });

        cliente.Activo = false;
        await _clientes.GuardarCambiosAsync();
        return NoContent();
    }

    /// <summary>Reactiva un cliente previamente desactivado.</summary>
    [HttpPost("{id:int}/activar")]
    public async Task<IActionResult> Activar(int id)
    {
        var cliente = await _clientes.ObtenerPorIdAsync(id);
        if (cliente is null) return NotFound();
        if (cliente.Activo) return NoContent();

        cliente.Activo = true;
        await _clientes.GuardarCambiosAsync();
        return NoContent();
    }

    private static ClienteDto Map(Cliente c) => new(c.Id, c.Nombre, c.Apellido, c.Email, c.Telefono, c.Activo);
}
