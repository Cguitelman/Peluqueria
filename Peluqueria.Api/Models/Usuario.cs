using System.ComponentModel.DataAnnotations;

namespace Peluqueria.Api.Models;

public class Usuario
{
    public int Id { get; set; }

    [Required, StringLength(50)]
    public string NombreUsuario { get; set; } = string.Empty;

    [Required]
    public string PasswordHash { get; set; } = string.Empty;

    [Required, StringLength(80)]
    public string NombreMostrado { get; set; } = string.Empty;

    public RolUsuario Rol { get; set; }

    public bool Activo { get; set; } = true;

    public ICollection<SesionUsuario> Sesiones { get; set; } = new List<SesionUsuario>();
}
