using System.Globalization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Peluqueria.Web.Models;
using Peluqueria.Web.Services;

namespace Peluqueria.Web.Pages;

public class IndexModel : PageModel
{
    private readonly ApiClient _api;
    public IndexModel(ApiClient api) => _api = api;

    public int ClientesActivos { get; set; }
    public int ProfesionalesActivos { get; set; }
    public int ServiciosActivos { get; set; }
    public int TurnosHoy { get; set; }
    public int TurnosPendientesHoy { get; set; }
    public int TurnosConfirmadosHoy { get; set; }
    public int TurnosFinalizadosHoy { get; set; }
    public int TurnosCanceladosHoy { get; set; }
    public List<TurnoDto> AgendaHoy { get; set; } = new();
    public TurnoDto? ProximoTurno { get; set; }
    public string? ErrorCarga { get; set; }

    public string FechaActual => DateTime.Today.ToString("dddd d 'de' MMMM 'de' yyyy", new CultureInfo("es-AR"));

    public async Task OnGetAsync()
    {
        try
        {
            var clientes = await _api.GetAsync<ClienteDto>("api/clientes");
            var profesionales = await _api.GetAsync<ProfesionalDto>("api/profesionales");
            var servicios = await _api.GetAsync<ServicioDto>("api/servicios");
            var turnos = await _api.GetAsync<TurnoDto>($"api/turnos?fecha={DateTime.Today:yyyy-MM-dd}");

            AgendaHoy = turnos
                .Where(t => t.Activo)
                .OrderBy(t => t.FechaHora)
                .ToList();

            ClientesActivos = clientes.Count;
            ProfesionalesActivos = profesionales.Count;
            ServiciosActivos = servicios.Count;
            TurnosHoy = AgendaHoy.Count;
            TurnosPendientesHoy = AgendaHoy.Count(t => t.Estado == EstadoTurno.Pendiente);
            TurnosConfirmadosHoy = AgendaHoy.Count(t => t.Estado == EstadoTurno.Confirmado);
            TurnosFinalizadosHoy = AgendaHoy.Count(t => t.Estado == EstadoTurno.Finalizado);
            TurnosCanceladosHoy = AgendaHoy.Count(t => t.Estado == EstadoTurno.Cancelado);
            ProximoTurno = AgendaHoy.FirstOrDefault(t =>
                t.FechaHora > DateTime.Now &&
                t.Estado != EstadoTurno.Cancelado &&
                t.Estado != EstadoTurno.Finalizado);
        }
        catch (ApiException ex)
        {
            ErrorCarga = ex.Message;
        }
    }
}
