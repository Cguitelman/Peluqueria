using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Peluqueria.Api.Data;

#nullable disable

namespace Peluqueria.Api.Migrations;

[DbContext(typeof(ApplicationDbContext))]
partial class ApplicationDbContextModelSnapshot : ModelSnapshot
{
    protected override void BuildModel(ModelBuilder modelBuilder)
    {
#pragma warning disable 612, 618
        modelBuilder
            .HasAnnotation("ProductVersion", "8.0.0")
            .HasAnnotation("Relational:MaxIdentifierLength", 128);

        SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

        modelBuilder.Entity("Peluqueria.Api.Models.Cliente", b =>
        {
            b.Property<int>("Id").ValueGeneratedOnAdd().HasColumnType("int");
            SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));
            b.Property<bool>("Activo").ValueGeneratedOnAdd().HasColumnType("bit").HasDefaultValue(true);
            b.Property<string>("Apellido").IsRequired().HasMaxLength(60).HasColumnType("nvarchar(60)");
            b.Property<string>("Email").IsRequired().HasMaxLength(120).HasColumnType("nvarchar(120)");
            b.Property<string>("Nombre").IsRequired().HasMaxLength(60).HasColumnType("nvarchar(60)");
            b.Property<string>("Telefono").IsRequired().HasMaxLength(30).HasColumnType("nvarchar(30)");
            b.HasKey("Id");
            b.HasIndex("Email").IsUnique();
            b.ToTable("Clientes");
        });

        modelBuilder.Entity("Peluqueria.Api.Models.Profesional", b =>
        {
            b.Property<int>("Id").ValueGeneratedOnAdd().HasColumnType("int");
            SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));
            b.Property<bool>("Activo").ValueGeneratedOnAdd().HasColumnType("bit").HasDefaultValue(true);
            b.Property<string>("Apellido").IsRequired().HasMaxLength(60).HasColumnType("nvarchar(60)");
            b.Property<string>("Email").IsRequired().HasMaxLength(120).HasColumnType("nvarchar(120)");
            b.Property<string>("Especialidad").IsRequired().HasMaxLength(80).HasColumnType("nvarchar(80)");
            b.Property<string>("Nombre").IsRequired().HasMaxLength(60).HasColumnType("nvarchar(60)");
            b.HasKey("Id");
            b.HasIndex("Email").IsUnique();
            b.ToTable("Profesionales");
        });

        modelBuilder.Entity("Peluqueria.Api.Models.Servicio", b =>
        {
            b.Property<int>("Id").ValueGeneratedOnAdd().HasColumnType("int");
            SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));
            b.Property<bool>("Activo").ValueGeneratedOnAdd().HasColumnType("bit").HasDefaultValue(true);
            b.Property<string>("Descripcion").IsRequired().HasMaxLength(250).HasColumnType("nvarchar(250)");
            b.Property<decimal>("DuracionHoras").HasPrecision(5, 2).HasColumnType("decimal(5,2)");
            b.Property<string>("Nombre").IsRequired().HasMaxLength(80).HasColumnType("nvarchar(80)");
            b.Property<decimal>("Precio").HasPrecision(18, 2).HasColumnType("decimal(18,2)");
            b.HasKey("Id");
            b.ToTable("Servicios");
        });

        modelBuilder.Entity("Peluqueria.Api.Models.Usuario", b =>
        {
            b.Property<int>("Id").ValueGeneratedOnAdd().HasColumnType("int");
            SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));
            b.Property<bool>("Activo").ValueGeneratedOnAdd().HasColumnType("bit").HasDefaultValue(true);
            b.Property<string>("NombreMostrado").IsRequired().HasMaxLength(80).HasColumnType("nvarchar(80)");
            b.Property<string>("NombreUsuario").IsRequired().HasMaxLength(50).HasColumnType("nvarchar(50)");
            b.Property<string>("PasswordHash").IsRequired().HasColumnType("nvarchar(max)");
            b.Property<string>("Rol").IsRequired().HasColumnType("nvarchar(max)");
            b.HasKey("Id");
            b.HasIndex("NombreUsuario").IsUnique();
            b.ToTable("Usuarios");
        });

        modelBuilder.Entity("Peluqueria.Api.Models.Turno", b =>
        {
            b.Property<int>("Id").ValueGeneratedOnAdd().HasColumnType("int");
            SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));
            b.Property<bool>("Activo").ValueGeneratedOnAdd().HasColumnType("bit").HasDefaultValue(true);
            b.Property<int>("ClienteId").HasColumnType("int");
            b.Property<int>("Estado").HasColumnType("int");
            b.Property<DateTime>("FechaHora").HasColumnType("datetime2");
            b.Property<int>("ProfesionalId").HasColumnType("int");
            b.Property<int>("ServicioId").HasColumnType("int");
            b.HasKey("Id");
            b.HasIndex("ClienteId");
            b.HasIndex("ProfesionalId");
            b.HasIndex("ServicioId");
            b.ToTable("Turnos");
        });

        modelBuilder.Entity("Peluqueria.Api.Models.SesionUsuario", b =>
        {
            b.Property<int>("Id").ValueGeneratedOnAdd().HasColumnType("int");
            SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));
            b.Property<bool>("Activa").ValueGeneratedOnAdd().HasColumnType("bit").HasDefaultValue(true);
            b.Property<DateTime>("FechaExpiracion").HasColumnType("datetime2");
            b.Property<string>("TokenHash").IsRequired().HasMaxLength(64).HasColumnType("nvarchar(64)");
            b.Property<int>("UsuarioId").HasColumnType("int");
            b.HasKey("Id");
            b.HasIndex("TokenHash").IsUnique();
            b.HasIndex("UsuarioId");
            b.ToTable("SesionesUsuario");
        });

        modelBuilder.Entity("Peluqueria.Api.Models.SesionUsuario", b =>
        {
            b.HasOne("Peluqueria.Api.Models.Usuario", "Usuario")
                .WithMany("Sesiones")
                .HasForeignKey("UsuarioId")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();
            b.Navigation("Usuario");
        });

        modelBuilder.Entity("Peluqueria.Api.Models.Turno", b =>
        {
            b.HasOne("Peluqueria.Api.Models.Cliente", "Cliente").WithMany("Turnos").HasForeignKey("ClienteId").OnDelete(DeleteBehavior.Restrict).IsRequired();
            b.HasOne("Peluqueria.Api.Models.Profesional", "Profesional").WithMany("Turnos").HasForeignKey("ProfesionalId").OnDelete(DeleteBehavior.Restrict).IsRequired();
            b.HasOne("Peluqueria.Api.Models.Servicio", "Servicio").WithMany("Turnos").HasForeignKey("ServicioId").OnDelete(DeleteBehavior.Restrict).IsRequired();
            b.Navigation("Cliente");
            b.Navigation("Profesional");
            b.Navigation("Servicio");
        });

        modelBuilder.Entity("Peluqueria.Api.Models.Cliente", b => b.Navigation("Turnos"));
        modelBuilder.Entity("Peluqueria.Api.Models.Profesional", b => b.Navigation("Turnos"));
        modelBuilder.Entity("Peluqueria.Api.Models.Servicio", b => b.Navigation("Turnos"));
        modelBuilder.Entity("Peluqueria.Api.Models.Usuario", b => b.Navigation("Sesiones"));
#pragma warning restore 612, 618
    }
}
