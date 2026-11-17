using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Peluqueria.Web.Models;
using Peluqueria.Web.Services;

namespace Peluqueria.Web.Pages.Profesionales;

public class IndexModel : PageModel
{
    private readonly ApiClient _api;
    public IndexModel(ApiClient api) => _api = api;

    public List<ProfesionalDto> Profesionales { get; set; } = new();
    [BindProperty] public GuardarProfesionalDto Nuevo { get; set; } = new();
    [BindProperty(SupportsGet = true)] public string? FiltroNombre { get; set; }
    [BindProperty(SupportsGet = true)] public string? FiltroApellido { get; set; }
    [BindProperty(SupportsGet = true)] public string? FiltroEspecialidad { get; set; }
    [BindProperty(SupportsGet = true)] public string? FiltroRegistro { get; set; } = "Todos";

    public async Task OnGetAsync() => await CargarAsync();

    public async Task<IActionResult> OnPostCrearAsync()
    {
        if (!User.IsInRole("Administrador")) return Forbid();
        if (!ModelState.IsValid) { await CargarAsync(); return Page(); }
        try
        {
            await _api.PostAsync("api/profesionales", Nuevo);
            TempData["Success"] = "Profesional creado correctamente.";
            return RedirectToPage();
        }
        catch (ApiException ex) { ModelState.AddModelError(string.Empty, ex.Message); await CargarAsync(); return Page(); }
    }

    public async Task<IActionResult> OnPostEliminarAsync(int id)
    {
        if (!User.IsInRole("Administrador")) return Forbid();
        try
        {
            await _api.DeleteAsync($"api/profesionales/{id}");
            TempData["Success"] = "Profesional desactivado correctamente.";
            return RedirectToPage();
        }
        catch (ApiException ex) { TempData["Error"] = ex.Message; return RedirectToPage(); }
    }

    public async Task<IActionResult> OnPostActivarAsync(int id)
    {
        if (!User.IsInRole("Administrador")) return Forbid();
        try
        {
            await _api.PostAsync($"api/profesionales/{id}/activar");
            TempData["Success"] = "Profesional activado correctamente.";
            return RedirectToPage();
        }
        catch (ApiException ex) { TempData["Error"] = ex.Message; return RedirectToPage(); }
    }

    private async Task CargarAsync()
    {
        var parametros = new List<string> { "incluirInactivos=true" };
        if (!string.IsNullOrWhiteSpace(FiltroNombre)) parametros.Add($"nombre={Uri.EscapeDataString(FiltroNombre.Trim())}");
        if (!string.IsNullOrWhiteSpace(FiltroApellido)) parametros.Add($"apellido={Uri.EscapeDataString(FiltroApellido.Trim())}");
        if (!string.IsNullOrWhiteSpace(FiltroEspecialidad)) parametros.Add($"especialidad={Uri.EscapeDataString(FiltroEspecialidad.Trim())}");

        Profesionales = await _api.GetAsync<ProfesionalDto>($"api/profesionales?{string.Join("&", parametros)}");
        Profesionales = (FiltroRegistro ?? "Todos") switch
        {
            "Activos" => Profesionales.Where(p => p.Activo).ToList(),
            "Inactivos" => Profesionales.Where(p => !p.Activo).ToList(),
            _ => Profesionales
        };
    }
}
