namespace Peluqueria.Api.Configuracion;

public class AgendaOptions
{
    public const string Seccion = "Agenda";
    public int HoraApertura { get; set; } = 9;
    public int HoraCierre { get; set; } = 20;
    public int IntervaloMinutos { get; set; } = 30;
}
