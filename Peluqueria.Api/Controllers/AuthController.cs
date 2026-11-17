using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Peluqueria.Api.DTOs;
using Peluqueria.Api.Models;
using Peluqueria.Api.Repositories;
using Peluqueria.Api.Security;

namespace Peluqueria.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IUsuarioRepository _usuarios;
    private readonly ISesionRepository _sesiones;
    private readonly IPasswordHasher<Usuario> _passwordHasher;

    public AuthController(
        IUsuarioRepository usuarios,
        ISesionRepository sesiones,
        IPasswordHasher<Usuario> passwordHasher)
    {
        _usuarios = usuarios;
        _sesiones = sesiones;
        _passwordHasher = passwordHasher;
    }

    /// <summary>Inicia sesión y devuelve un token Bearer.</summary>
    [AllowAnonymous]
    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<LoginResponseDto>> Login(LoginRequestDto dto)
    {
        var usuario = await _usuarios.ObtenerActivoPorNombreAsync(dto.Usuario.Trim().ToLowerInvariant());
        if (usuario is null)
            return Unauthorized(new { mensaje = "Usuario o contraseña incorrectos." });

        var resultado = _passwordHasher.VerifyHashedPassword(usuario, usuario.PasswordHash, dto.Password);
        if (resultado == PasswordVerificationResult.Failed)
            return Unauthorized(new { mensaje = "Usuario o contraseña incorrectos." });

        // Limpia sesiones revocadas o vencidas antes de crear una nueva.
        await _sesiones.LimpiarVencidasAsync();

        var token = TokenService.GenerarToken();
        var expira = DateTime.UtcNow.AddHours(8);
        var sesion = new SesionUsuario
        {
            UsuarioId = usuario.Id,
            TokenHash = TokenService.CalcularHash(token),
            FechaExpiracion = expira,
            Activa = true
        };

        await _sesiones.AgregarAsync(sesion);
        await _sesiones.GuardarCambiosAsync();

        return Ok(new LoginResponseDto(
            token,
            expira,
            usuario.NombreUsuario,
            usuario.NombreMostrado,
            usuario.Rol.ToString()));
    }

    /// <summary>Devuelve los datos del usuario autenticado.</summary>
    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<UsuarioActualDto>> Me()
    {
        var idTexto = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(idTexto, out var id))
            return Unauthorized();

        var usuario = await _usuarios.ObtenerActivoPorIdAsync(id);
        if (usuario is null)
            return Unauthorized();

        return Ok(new UsuarioActualDto(usuario.Id, usuario.NombreUsuario, usuario.NombreMostrado, usuario.Rol.ToString()));
    }

    /// <summary>Cierra la sesión actual invalidando el token.</summary>
    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        var authorization = Request.Headers["Authorization"].ToString();
        var token = authorization.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
            ? authorization["Bearer ".Length..].Trim()
            : string.Empty;

        if (!string.IsNullOrWhiteSpace(token))
        {
            await _sesiones.RevocarAsync(TokenService.CalcularHash(token));
            await _sesiones.GuardarCambiosAsync();
        }

        return NoContent();
    }
}
