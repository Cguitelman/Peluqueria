using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Peluqueria.Api.Models;

namespace Peluqueria.Api.Data;

public static class DbInitializer
{
    public static async Task InicializarAsync(ApplicationDbContext db, IPasswordHasher<Usuario> passwordHasher)
    {
        // A partir de esta versión la estructura se administra mediante migraciones de Entity Framework.
        await db.Database.MigrateAsync();

        if (await db.Usuarios.AnyAsync())
            return;

        var admin = new Usuario
        {
            NombreUsuario = "admin",
            NombreMostrado = "Administrador",
            Rol = RolUsuario.Administrador,
            Activo = true
        };
        admin.PasswordHash = passwordHasher.HashPassword(admin, "Admin123!");

        var recepcion = new Usuario
        {
            NombreUsuario = "recepcion",
            NombreMostrado = "Recepción",
            Rol = RolUsuario.Recepcionista,
            Activo = true
        };
        recepcion.PasswordHash = passwordHasher.HashPassword(recepcion, "Recep123!");

        db.Usuarios.AddRange(admin, recepcion);
        await db.SaveChangesAsync();
    }
}
