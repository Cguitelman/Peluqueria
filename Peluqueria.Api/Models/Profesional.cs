using System.ComponentModel.DataAnnotations;

namespace Peluqueria.Api.Models;

public class Profesional
{
    public int Id { get; set; }

    [Required, StringLength(60)]
    public string Nombre { get; set; } = string.Empty;

    [Required, StringLength(60)]
    public string Apellido { get; set; } = string.Empty;

    [Required, EmailAddress, StringLength(120)]
    public string Email { get; set; } = string.Empty;

    [Required, StringLength(80)]
    public string Especialidad { get; set; } = string.Empty;

    public bool Activo { get; set; } = true;

    public ICollection<Turno> Turnos { get; set; } = new List<Turno>();
}
