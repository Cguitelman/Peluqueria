using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Peluqueria.Web.Models;
using Peluqueria.Web.Services;

namespace Peluqueria.Web.Pages.Turnos;

public class IndexModel : PageModel
{
    private readonly ApiClient _api;
    private readonly int _horaApertura;
    private readonly int _horaCierre;
    private readonly int _intervaloMinutos;

    public IndexModel(ApiClient api, IConfiguration configuration)
    {
        _api = api;
        _horaApertura = configuration.GetValue("Agenda:HoraApertura", 9);
        _horaCierre = configuration.GetValue("Agenda:HoraCierre", 20);
        _intervaloMinutos = configuration.GetValue("Agenda:IntervaloMinutos", 30);
    }

    public List<TurnoDto> Turnos { get; set; } = new();
    public List<ClienteDto> Clientes { get; set; } = new();
    public List<ProfesionalDto> Profesionales { get; set; } = new();
    public List<ServicioDto> Servicios { get; set; } = new();
    public List<string> HorariosDisponibles { get; set; } = new();

    [BindProperty] public CrearTurnoDto Nuevo { get; set; } = new();
    [BindProperty] public DateTime FechaSeleccionada { get; set; }
    [BindProperty] public string HoraSeleccionada { get; set; } = string.Empty;

    [BindProperty(SupportsGet = true)] public string? FiltroCliente { get; set; }
    [BindProperty(SupportsGet = true)] public string? FiltroProfesional { get; set; }
    [BindProperty(SupportsGet = true)] public string? FiltroServicio { get; set; }
    [BindProperty(SupportsGet = true)] public DateTime? FechaDesde { get; set; }
    [BindProperty(SupportsGet = true)] public DateTime? FechaHasta { get; set; }
    [BindProperty(SupportsGet = true)] public EstadoTurno? FiltroEstado { get; set; }
    [BindProperty(SupportsGet = true)] public string? FiltroRegistro { get; set; } = "Todos";

    public string FechaMin => DateTime.Today.ToString("yyyy-MM-dd");
    public int IntervaloMinutos => _intervaloMinutos <= 0 ? 30 : _intervaloMinutos;
    public int HoraApertura => _horaApertura;
    public int HoraCierre => _horaCierre;
    public string HorarioAtencion => $"{_horaApertura:00}:00 a {_horaCierre:00}:00";

    public async Task OnGetAsync()
    {
        var proximo = ProximoHorarioValido(DateTime.Now);
        FechaSeleccionada = proximo.Date;
        HoraSeleccionada = proximo.ToString("HH:mm");
        await CargarAsync();
    }

    public async Task<IActionResult> OnPostCrearAsync()
    {
        if (!TimeSpan.TryParse(HoraSeleccionada, out var hora))
        {
            ModelState.AddModelError(nameof(HoraSeleccionada), "Seleccioná un horario válido.");
        }
        else
        {
            Nuevo.FechaHora = FechaSeleccionada.Date.Add(hora);

            if (FechaSeleccionada.Date < DateTime.Today)
                ModelState.AddModelError(nameof(FechaSeleccionada), "La fecha no puede ser anterior a hoy.");

            if (Nuevo.FechaHora <= DateTime.Now)
                ModelState.AddModelError(nameof(HoraSeleccionada), "Elegí un horario posterior al momento actual.");
        }

        if (!ModelState.IsValid)
        {
            await CargarAsync();
            return Page();
        }

        try
        {
            await _api.PostAsync("api/turnos", Nuevo);
            TempData["Success"] = "Turno creado correctamente.";
            return RedirectToPage();
        }
        catch (ApiException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            await CargarAsync();
            return Page();
        }
    }

    public async Task<IActionResult> OnPostEliminarAsync(int id)
    {
        try
        {
            await _api.DeleteAsync($"api/turnos/{id}");
            TempData["Success"] = "Turno desactivado correctamente. Podés volver a activarlo desde este mismo listado.";
        }
        catch (ApiException ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostActivarAsync(int id)
    {
        try
        {
            await _api.PostAsync($"api/turnos/{id}/activar");
            TempData["Success"] = "Turno activado correctamente.";
        }
        catch (ApiException ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToPage();
    }

    private async Task CargarAsync()
    {
        CargarHorarios();

        if (FechaSeleccionada == default)
        {
            var proximo = ProximoHorarioValido(DateTime.Now);
            FechaSeleccionada = proximo.Date;
            HoraSeleccionada = proximo.ToString("HH:mm");
        }

        if (string.IsNullOrWhiteSpace(HoraSeleccionada))
            HoraSeleccionada = HorariosDisponibles.FirstOrDefault() ?? "09:00";

        if (FechaDesde.HasValue && FechaHasta.HasValue && FechaHasta.Value.Date < FechaDesde.Value.Date)
            ModelState.AddModelError(string.Empty, "La fecha hasta no puede ser anterior a la fecha desde.");

        var parametros = new List<string> { "incluirInactivos=true" };
        if (!string.IsNullOrWhiteSpace(FiltroCliente)) parametros.Add($"cliente={Uri.EscapeDataString(FiltroCliente.Trim())}");
        if (!string.IsNullOrWhiteSpace(FiltroProfesional)) parametros.Add($"profesional={Uri.EscapeDataString(FiltroProfesional.Trim())}");
        if (!string.IsNullOrWhiteSpace(FiltroServicio)) parametros.Add($"servicio={Uri.EscapeDataString(FiltroServicio.Trim())}");
        if (FechaDesde.HasValue) parametros.Add($"fechaDesde={FechaDesde.Value:yyyy-MM-dd}");
        if (FechaHasta.HasValue) parametros.Add($"fechaHasta={FechaHasta.Value:yyyy-MM-dd}");
        if (FiltroEstado.HasValue) parametros.Add($"estado={FiltroEstado.Value}");

        if (!ModelState.IsValid && FechaDesde.HasValue && FechaHasta.HasValue && FechaHasta.Value.Date < FechaDesde.Value.Date)
            Turnos = new List<TurnoDto>();
        else
            Turnos = await _api.GetAsync<TurnoDto>($"api/turnos?{string.Join("&", parametros)}");

        Turnos = (FiltroRegistro ?? "Todos") switch
        {
            "Activos" => Turnos.Where(t => t.Activo).ToList(),
            "Inactivos" => Turnos.Where(t => !t.Activo).ToList(),
            _ => Turnos
        };

        Clientes = await _api.GetAsync<ClienteDto>("api/clientes");
        Profesionales = await _api.GetAsync<ProfesionalDto>("api/profesionales");
        Servicios = await _api.GetAsync<ServicioDto>("api/servicios");
    }

    private void CargarHorarios()
    {
        HorariosDisponibles.Clear();
        var intervalo = IntervaloMinutos;
        var actual = TimeSpan.FromHours(_horaApertura);
        var cierre = TimeSpan.FromHours(_horaCierre);

        while (actual < cierre)
        {
            HorariosDisponibles.Add(actual.ToString(@"hh\:mm"));
            actual = actual.Add(TimeSpan.FromMinutes(intervalo));
        }
    }

    private DateTime ProximoHorarioValido(DateTime ahora)
    {
        var intervalo = IntervaloMinutos;
        var apertura = new TimeSpan(_horaApertura, 0, 0);
        var cierre = new TimeSpan(_horaCierre, 0, 0);

        if (ahora.TimeOfDay < apertura)
            return ahora.Date.Add(apertura);

        if (ahora.TimeOfDay >= cierre)
            return ahora.Date.AddDays(1).Add(apertura);

        var baseHora = new DateTime(ahora.Year, ahora.Month, ahora.Day, ahora.Hour, ahora.Minute, 0);
        var minutosDesdeMedianoche = baseHora.Hour * 60 + baseHora.Minute;
        var aperturaMinutos = _horaApertura * 60;
        var transcurridos = Math.Max(0, minutosDesdeMedianoche - aperturaMinutos);
        var siguienteBloque = ((transcurridos / intervalo) + 1) * intervalo;
        var candidato = ahora.Date.AddMinutes(aperturaMinutos + siguienteBloque);

        if (candidato.TimeOfDay >= cierre)
            return ahora.Date.AddDays(1).Add(apertura);

        return candidato;
    }
}
