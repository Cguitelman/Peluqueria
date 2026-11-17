using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Peluqueria.Web.Models;
using Peluqueria.Web.Services;

namespace Peluqueria.Web.Pages.Turnos;

public class EditarModel : PageModel
{
    private readonly ApiClient _api;
    public EditarModel(ApiClient api) => _api = api;

    [BindProperty] public int Id { get; set; }
    [BindProperty] public ActualizarTurnoDto Input { get; set; } = new();
    public List<ClienteDto> Clientes { get; set; } = new();
    public List<ProfesionalDto> Profesionales { get; set; } = new();
    public List<ServicioDto> Servicios { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var turno = await _api.GetPorIdAsync<TurnoDto>($"api/turnos/{id}");
        if (turno is null) return NotFound();
        Id = id;
        Input = new ActualizarTurnoDto
        {
            ClienteId = turno.ClienteId,
            ProfesionalId = turno.ProfesionalId,
            ServicioId = turno.ServicioId,
            FechaHora = turno.FechaHora,
            Estado = turno.Estado
        };
        await CargarListasAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        // La API distingue entre reprogramar (debe ser futuro) y cambiar solo el estado.
        // Esto permite finalizar o cancelar un turno que ya ocurrió.
        if (!ModelState.IsValid) { await CargarListasAsync(); return Page(); }
        try
        {
            await _api.PutAsync($"api/turnos/{Id}", Input);
            TempData["Success"] = "Turno actualizado correctamente.";
            return RedirectToPage("Index");
        }
        catch (ApiException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            await CargarListasAsync();
            return Page();
        }
    }

    private async Task CargarListasAsync()
    {
        Clientes = await _api.GetAsync<ClienteDto>("api/clientes?incluirInactivos=true");
        Profesionales = await _api.GetAsync<ProfesionalDto>("api/profesionales?incluirInactivos=true");
        Servicios = await _api.GetAsync<ServicioDto>("api/servicios?incluirInactivos=true");
    }
}
