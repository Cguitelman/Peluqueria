using System.ComponentModel.DataAnnotations;

namespace Peluqueria.Web.Models;

public enum EstadoTurno
{
    Pendiente,
    Confirmado,
    Cancelado,
    Finalizado
}

public class ClienteDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
    public bool Activo { get; set; }
    public string NombreCompleto => $"{Nombre} {Apellido}";
}

public class GuardarClienteDto
{
    [Required, StringLength(60)] public string Nombre { get; set; } = string.Empty;
    [Required, StringLength(60)] public string Apellido { get; set; } = string.Empty;
    [Required, EmailAddress, StringLength(120)] public string Email { get; set; } = string.Empty;
    [Required, StringLength(30)] public string Telefono { get; set; } = string.Empty;
}

public class ProfesionalDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Especialidad { get; set; } = string.Empty;
    public bool Activo { get; set; }
    public string NombreCompleto => $"{Nombre} {Apellido}";
}

public class GuardarProfesionalDto
{
    [Required, StringLength(60)] public string Nombre { get; set; } = string.Empty;
    [Required, StringLength(60)] public string Apellido { get; set; } = string.Empty;
    [Required, EmailAddress, StringLength(120)] public string Email { get; set; } = string.Empty;
    [Required, StringLength(80)] public string Especialidad { get; set; } = string.Empty;
}

public class ServicioDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public decimal DuracionHoras { get; set; }
    public decimal Precio { get; set; }
    public bool Activo { get; set; }
}

public class GuardarServicioDto
{
    [Required, StringLength(80)] public string Nombre { get; set; } = string.Empty;
    [StringLength(250)] public string Descripcion { get; set; } = string.Empty;
    [Range(0.5, 10.0, ErrorMessage = "La duración debe estar entre 0,5 y 10 horas.")]
    public decimal DuracionHoras { get; set; } = 0.5m;
    [Range(0, 99999999)] public decimal Precio { get; set; }
}

public class TurnoDto
{
    public int Id { get; set; }
    public DateTime FechaHora { get; set; }
    public EstadoTurno Estado { get; set; }
    public int ClienteId { get; set; }
    public string Cliente { get; set; } = string.Empty;
    public int ProfesionalId { get; set; }
    public string Profesional { get; set; } = string.Empty;
    public int ServicioId { get; set; }
    public string Servicio { get; set; } = string.Empty;
    public decimal DuracionHoras { get; set; }
    public decimal Precio { get; set; }
    public bool Activo { get; set; }
}

public class CrearTurnoDto
{
    [Range(1, int.MaxValue)] public int ClienteId { get; set; }
    [Range(1, int.MaxValue)] public int ProfesionalId { get; set; }
    [Range(1, int.MaxValue)] public int ServicioId { get; set; }
    [Required] public DateTime FechaHora { get; set; }
}

public class ActualizarTurnoDto : CrearTurnoDto
{
    public EstadoTurno Estado { get; set; }
}

public class LoginRequestDto
{
    [Required] public string Usuario { get; set; } = string.Empty;
    [Required, DataType(DataType.Password)] public string Password { get; set; } = string.Empty;
}

public class LoginResponseDto
{
    public string Token { get; set; } = string.Empty;
    public DateTime Expira { get; set; }
    public string Usuario { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Rol { get; set; } = string.Empty;
}
