using System.ComponentModel.DataAnnotations;

namespace Peluqueria.Api.Models;

public class Servicio
{
    public int Id { get; set; }

    [Required, StringLength(80)]
    public string Nombre { get; set; } = string.Empty;

    [StringLength(250)]
    public string Descripcion { get; set; } = string.Empty;

    // La duración se expresa en horas y admite intervalos de media hora (0.5, 1, 1.5, etc.).
    [Range(0.5, 10.0)]
    public decimal DuracionHoras { get; set; }

    [Range(0, 99999999)]
    public decimal Precio { get; set; }

    // Se utiliza para borrado lógico.
    public bool Activo { get; set; } = true;

    public ICollection<Turno> Turnos { get; set; } = new List<Turno>();
}
