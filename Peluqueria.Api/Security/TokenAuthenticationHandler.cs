using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using Peluqueria.Api.Repositories;

namespace Peluqueria.Api.Security;

public class TokenAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public const string SchemeName = "Bearer";
    private readonly ISesionRepository _sesiones;

    public TokenAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        ISesionRepository sesiones) : base(options, logger, encoder)
    {
        _sesiones = sesiones;
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue("Authorization", out var authorization))
            return AuthenticateResult.NoResult();

        var valor = authorization.ToString();
        if (!valor.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            return AuthenticateResult.NoResult();

        var token = valor["Bearer ".Length..].Trim();
        if (string.IsNullOrWhiteSpace(token))
            return AuthenticateResult.Fail("Token vacío.");

        var tokenHash = TokenService.CalcularHash(token);
        var sesion = await _sesiones.ObtenerValidaPorHashAsync(tokenHash);
        if (sesion is null)
            return AuthenticateResult.Fail("Token inválido o vencido.");

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, sesion.Usuario.Id.ToString()),
            new Claim(ClaimTypes.Name, sesion.Usuario.NombreUsuario),
            new Claim("nombre", sesion.Usuario.NombreMostrado),
            new Claim(ClaimTypes.Role, sesion.Usuario.Rol.ToString())
        };

        var identity = new ClaimsIdentity(claims, SchemeName);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, SchemeName);
        return AuthenticateResult.Success(ticket);
    }
}
