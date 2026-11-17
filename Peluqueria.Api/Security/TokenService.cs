using System.Security.Cryptography;
using System.Text;

namespace Peluqueria.Api.Security;

public static class TokenService
{
    public static string GenerarToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(32);
        return Convert.ToHexString(bytes);
    }

    public static string CalcularHash(string token)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToHexString(bytes);
    }
}
