namespace Peluqueria.Api.Models;

public class Turno
{
    public int Id { get; set; }
    public DateTime FechaHora { get; set; }
    public EstadoTurno Estado { get; private set; } = EstadoTurno.Pendiente;

    // Se utiliza para borrado lógico.
    public bool Activo { get; set; } = true;

    public int ClienteId { get; set; }
    public Cliente Cliente { get; set; } = null!;

    public int ProfesionalId { get; set; }
    public Profesional Profesional { get; set; } = null!;

    public int ServicioId { get; set; }
    public Servicio Servicio { get; set; } = null!;

    public void CambiarEstado(EstadoTurno nuevoEstado) => Estado = nuevoEstado;
    public void Confirmar() => Estado = EstadoTurno.Confirmado;
    public void Cancelar() => Estado = EstadoTurno.Cancelado;
}
