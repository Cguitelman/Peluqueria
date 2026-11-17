using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Peluqueria.Web.Models;
using Peluqueria.Web.Services;

namespace Peluqueria.Web.Pages.Servicios;

public class IndexModel : PageModel
{
    private readonly ApiClient _api;
    public IndexModel(ApiClient api) => _api = api;

    public List<ServicioDto> Servicios { get; set; } = new();
    [BindProperty] public GuardarServicioDto Nuevo { get; set; } = new();
    [BindProperty(SupportsGet = true)] public string? FiltroNombre { get; set; }
    [BindProperty(SupportsGet = true)] public string? FiltroRegistro { get; set; } = "Todos";

    public async Task OnGetAsync() => await CargarAsync();

    public async Task<IActionResult> OnPostCrearAsync()
    {
        if (!User.IsInRole("Administrador")) return Forbid();
        ValidarMediaHora(Nuevo.DuracionHoras);
        if (!ModelState.IsValid) { await CargarAsync(); return Page(); }
        try
        {
            await _api.PostAsync("api/servicios", Nuevo);
            TempData["Success"] = "Servicio creado correctamente.";
            return RedirectToPage();
        }
        catch (ApiException ex) { ModelState.AddModelError(string.Empty, ex.Message); await CargarAsync(); return Page(); }
    }

    public async Task<IActionResult> OnPostEliminarAsync(int id)
    {
        if (!User.IsInRole("Administrador")) return Forbid();
        try
        {
            await _api.DeleteAsync($"api/servicios/{id}");
            TempData["Success"] = "Servicio desactivado correctamente.";
            return RedirectToPage();
        }
        catch (ApiException ex) { TempData["Error"] = ex.Message; return RedirectToPage(); }
    }

    public async Task<IActionResult> OnPostActivarAsync(int id)
    {
        if (!User.IsInRole("Administrador")) return Forbid();
        try
        {
            await _api.PostAsync($"api/servicios/{id}/activar");
            TempData["Success"] = "Servicio activado correctamente.";
            return RedirectToPage();
        }
        catch (ApiException ex) { TempData["Error"] = ex.Message; return RedirectToPage(); }
    }

    private void ValidarMediaHora(decimal horas)
    {
        if (horas * 2m != decimal.Truncate(horas * 2m))
            ModelState.AddModelError("Nuevo.DuracionHoras", "Usá intervalos de media hora: 0,5; 1; 1,5; 2; etc.");
    }

    private async Task CargarAsync()
    {
        var parametros = new List<string> { "incluirInactivos=true" };
        if (!string.IsNullOrWhiteSpace(FiltroNombre)) parametros.Add($"nombre={Uri.EscapeDataString(FiltroNombre.Trim())}");

        Servicios = await _api.GetAsync<ServicioDto>($"api/servicios?{string.Join("&", parametros)}");
        Servicios = (FiltroRegistro ?? "Todos") switch
        {
            "Activos" => Servicios.Where(s => s.Activo).ToList(),
            "Inactivos" => Servicios.Where(s => !s.Activo).ToList(),
            _ => Servicios
        };
    }
}
