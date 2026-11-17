using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Peluqueria.Api.Data;

#nullable disable

namespace Peluqueria.Api.Migrations;

[DbContext(typeof(ApplicationDbContext))]
[Migration("20260929230000_InitialSqlServer")]
public partial class InitialSqlServer : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Clientes",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                Nombre = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                Apellido = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                Email = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                Telefono = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                Activo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
            },
            constraints: table => table.PrimaryKey("PK_Clientes", x => x.Id));

        migrationBuilder.CreateTable(
            name: "Profesionales",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                Nombre = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                Apellido = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                Email = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                Especialidad = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                Activo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
            },
            constraints: table => table.PrimaryKey("PK_Profesionales", x => x.Id));

        migrationBuilder.CreateTable(
            name: "Servicios",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                Nombre = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                Descripcion = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                DuracionHoras = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                Precio = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                Activo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
            },
            constraints: table => table.PrimaryKey("PK_Servicios", x => x.Id));

        migrationBuilder.CreateTable(
            name: "Usuarios",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                NombreUsuario = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                NombreMostrado = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                Rol = table.Column<string>(type: "nvarchar(max)", nullable: false),
                Activo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
            },
            constraints: table => table.PrimaryKey("PK_Usuarios", x => x.Id));

        migrationBuilder.CreateTable(
            name: "Turnos",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                FechaHora = table.Column<DateTime>(type: "datetime2", nullable: false),
                Estado = table.Column<int>(type: "int", nullable: false),
                Activo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                ClienteId = table.Column<int>(type: "int", nullable: false),
                ProfesionalId = table.Column<int>(type: "int", nullable: false),
                ServicioId = table.Column<int>(type: "int", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Turnos", x => x.Id);
                table.ForeignKey("FK_Turnos_Clientes_ClienteId", x => x.ClienteId, "Clientes", "Id", onDelete: ReferentialAction.Restrict);
                table.ForeignKey("FK_Turnos_Profesionales_ProfesionalId", x => x.ProfesionalId, "Profesionales", "Id", onDelete: ReferentialAction.Restrict);
                table.ForeignKey("FK_Turnos_Servicios_ServicioId", x => x.ServicioId, "Servicios", "Id", onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "SesionesUsuario",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                TokenHash = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                FechaExpiracion = table.Column<DateTime>(type: "datetime2", nullable: false),
                Activa = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                UsuarioId = table.Column<int>(type: "int", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_SesionesUsuario", x => x.Id);
                table.ForeignKey("FK_SesionesUsuario_Usuarios_UsuarioId", x => x.UsuarioId, "Usuarios", "Id", onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(name: "IX_Clientes_Email", table: "Clientes", column: "Email", unique: true);
        migrationBuilder.CreateIndex(name: "IX_Profesionales_Email", table: "Profesionales", column: "Email", unique: true);
        migrationBuilder.CreateIndex(name: "IX_SesionesUsuario_TokenHash", table: "SesionesUsuario", column: "TokenHash", unique: true);
        migrationBuilder.CreateIndex(name: "IX_SesionesUsuario_UsuarioId", table: "SesionesUsuario", column: "UsuarioId");
        migrationBuilder.CreateIndex(name: "IX_Turnos_ClienteId", table: "Turnos", column: "ClienteId");
        migrationBuilder.CreateIndex(name: "IX_Turnos_ProfesionalId", table: "Turnos", column: "ProfesionalId");
        migrationBuilder.CreateIndex(name: "IX_Turnos_ServicioId", table: "Turnos", column: "ServicioId");
        migrationBuilder.CreateIndex(name: "IX_Usuarios_NombreUsuario", table: "Usuarios", column: "NombreUsuario", unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "SesionesUsuario");
        migrationBuilder.DropTable(name: "Turnos");
        migrationBuilder.DropTable(name: "Usuarios");
        migrationBuilder.DropTable(name: "Clientes");
        migrationBuilder.DropTable(name: "Profesionales");
        migrationBuilder.DropTable(name: "Servicios");
    }
}
