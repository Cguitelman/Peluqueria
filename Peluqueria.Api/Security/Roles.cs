namespace Peluqueria.Api.Security;

public static class Roles
{
    public const string Administrador = "Administrador";
    public const string Recepcionista = "Recepcionista";
    public const string Personal = Administrador + "," + Recepcionista;
}
