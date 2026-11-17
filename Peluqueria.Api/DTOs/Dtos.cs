using System.ComponentModel.DataAnnotations;
using Peluqueria.Api.Models;

namespace Peluqueria.Api.DTOs;

public record ClienteDto(int Id, string Nombre, string Apellido, string Email, string Telefono, bool Activo)
{
    public string NombreCompleto => $"{Nombre} {Apellido}";
}

public class GuardarClienteDto
{
    [Required, StringLength(60)] public string Nombre { get; set; } = string.Empty;
    [Required, StringLength(60)] public string Apellido { get; set; } = string.Empty;
    [Required, EmailAddress, StringLength(120)] public string Email { get; set; } = string.Empty;
    [Required, StringLength(30)] public string Telefono { get; set; } = string.Empty;
}

public record ProfesionalDto(int Id, string Nombre, string Apellido, string Email, string Especialidad, bool Activo)
{
    public string NombreCompleto => $"{Nombre} {Apellido}";
}

public class GuardarProfesionalDto
{
    [Required, StringLength(60)] public string Nombre { get; set; } = string.Empty;
    [Required, StringLength(60)] public string Apellido { get; set; } = string.Empty;
    [Required, EmailAddress, StringLength(120)] public string Email { get; set; } = string.Empty;
    [Required, StringLength(80)] public string Especialidad { get; set; } = string.Empty;
}

public record ServicioDto(int Id, string Nombre, string Descripcion, decimal DuracionHoras, decimal Precio, bool Activo);

public class GuardarServicioDto
{
    [Required, StringLength(80)] public string Nombre { get; set; } = string.Empty;
    [StringLength(250)] public string Descripcion { get; set; } = string.Empty;
    [Range(0.5, 10.0)] public decimal DuracionHoras { get; set; }
    [Range(0, 99999999)] public decimal Precio { get; set; }
}

public record TurnoDto(
    int Id,
    DateTime FechaHora,
    EstadoTurno Estado,
    int ClienteId,
    string Cliente,
    int ProfesionalId,
    string Profesional,
    int ServicioId,
    string Servicio,
    decimal DuracionHoras,
    decimal Precio,
    bool Activo);

public class CrearTurnoDto
{
    [Range(1, int.MaxValue)] public int ClienteId { get; set; }
    [Range(1, int.MaxValue)] public int ProfesionalId { get; set; }
    [Range(1, int.MaxValue)] public int ServicioId { get; set; }
    public DateTime FechaHora { get; set; }
}

public class ActualizarTurnoDto : CrearTurnoDto
{
    [Required] public EstadoTurno Estado { get; set; }
}

public class LoginRequestDto
{
    [Required, StringLength(50)] public string Usuario { get; set; } = string.Empty;
    [Required, StringLength(100)] public string Password { get; set; } = string.Empty;
}

public record LoginResponseDto(
    string Token,
    DateTime Expira,
    string Usuario,
    string Nombre,
    string Rol);

public record UsuarioActualDto(int Id, string Usuario, string Nombre, string Rol);
