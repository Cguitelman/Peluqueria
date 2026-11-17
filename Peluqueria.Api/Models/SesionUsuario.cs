using System.ComponentModel.DataAnnotations;

namespace Peluqueria.Api.Models;

public class SesionUsuario
{
    public int Id { get; set; }

    [Required, StringLength(64)]
    public string TokenHash { get; set; } = string.Empty;

    public DateTime FechaExpiracion { get; set; }
    public bool Activa { get; set; } = true;

    public int UsuarioId { get; set; }
    public Usuario Usuario { get; set; } = null!;
}
