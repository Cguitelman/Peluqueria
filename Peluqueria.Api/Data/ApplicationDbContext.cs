using Microsoft.EntityFrameworkCore;
using Peluqueria.Api.Models;

namespace Peluqueria.Api.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<Profesional> Profesionales => Set<Profesional>();
    public DbSet<Servicio> Servicios => Set<Servicio>();
    public DbSet<Turno> Turnos => Set<Turno>();
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<SesionUsuario> SesionesUsuario => Set<SesionUsuario>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Cliente>().Property(c => c.Activo).HasDefaultValue(true);
        modelBuilder.Entity<Profesional>().Property(p => p.Activo).HasDefaultValue(true);
        modelBuilder.Entity<Servicio>().Property(s => s.Activo).HasDefaultValue(true);
        modelBuilder.Entity<Turno>().Property(t => t.Activo).HasDefaultValue(true);
        modelBuilder.Entity<Usuario>().Property(u => u.Activo).HasDefaultValue(true);
        modelBuilder.Entity<SesionUsuario>().Property(s => s.Activa).HasDefaultValue(true);

        modelBuilder.Entity<Cliente>()
            .HasIndex(c => c.Email)
            .IsUnique();

        modelBuilder.Entity<Profesional>()
            .HasIndex(p => p.Email)
            .IsUnique();

        modelBuilder.Entity<Usuario>()
            .HasIndex(u => u.NombreUsuario)
            .IsUnique();

        modelBuilder.Entity<SesionUsuario>()
            .HasIndex(s => s.TokenHash)
            .IsUnique();

        modelBuilder.Entity<Usuario>()
            .Property(u => u.Rol)
            .HasConversion<string>();

        modelBuilder.Entity<Servicio>()
            .Property(s => s.DuracionHoras)
            .HasPrecision(5, 2);

        modelBuilder.Entity<Servicio>()
            .Property(s => s.Precio)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Turno>()
            .HasOne(t => t.Cliente)
            .WithMany(c => c.Turnos)
            .HasForeignKey(t => t.ClienteId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Turno>()
            .HasOne(t => t.Profesional)
            .WithMany(p => p.Turnos)
            .HasForeignKey(t => t.ProfesionalId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Turno>()
            .HasOne(t => t.Servicio)
            .WithMany(s => s.Turnos)
            .HasForeignKey(t => t.ServicioId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<SesionUsuario>()
            .HasOne(s => s.Usuario)
            .WithMany(u => u.Sesiones)
            .HasForeignKey(s => s.UsuarioId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
