using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Peluqueria.Web.Models;
using Peluqueria.Web.Services;

namespace Peluqueria.Web.Pages.Clientes;

public class IndexModel : PageModel
{
    private readonly ApiClient _api;
    public IndexModel(ApiClient api) => _api = api;

    public List<ClienteDto> Clientes { get; set; } = new();
    [BindProperty] public GuardarClienteDto Nuevo { get; set; } = new();
    [BindProperty(SupportsGet = true)] public string? FiltroNombre { get; set; }
    [BindProperty(SupportsGet = true)] public string? FiltroApellido { get; set; }
    [BindProperty(SupportsGet = true)] public string? FiltroRegistro { get; set; } = "Todos";

    public async Task OnGetAsync() => await CargarAsync();

    public async Task<IActionResult> OnPostCrearAsync()
    {
        if (!ModelState.IsValid) { await CargarAsync(); return Page(); }
        try
        {
            await _api.PostAsync("api/clientes", Nuevo);
            TempData["Success"] = "Cliente creado correctamente.";
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
            await _api.DeleteAsync($"api/clientes/{id}");
            TempData["Success"] = "Cliente desactivado correctamente.";
            return RedirectToPage();
        }
        catch (ApiException ex)
        {
            TempData["Error"] = ex.Message;
            return RedirectToPage();
        }
    }

    public async Task<IActionResult> OnPostActivarAsync(int id)
    {
        try
        {
            await _api.PostAsync($"api/clientes/{id}/activar");
            TempData["Success"] = "Cliente activado correctamente.";
            return RedirectToPage();
        }
        catch (ApiException ex)
        {
            TempData["Error"] = ex.Message;
            return RedirectToPage();
        }
    }

    private async Task CargarAsync()
    {
        var parametros = new List<string> { "incluirInactivos=true" };
        if (!string.IsNullOrWhiteSpace(FiltroNombre)) parametros.Add($"nombre={Uri.EscapeDataString(FiltroNombre.Trim())}");
        if (!string.IsNullOrWhiteSpace(FiltroApellido)) parametros.Add($"apellido={Uri.EscapeDataString(FiltroApellido.Trim())}");

        Clientes = await _api.GetAsync<ClienteDto>($"api/clientes?{string.Join("&", parametros)}");
        Clientes = (FiltroRegistro ?? "Todos") switch
        {
            "Activos" => Clientes.Where(c => c.Activo).ToList(),
            "Inactivos" => Clientes.Where(c => !c.Activo).ToList(),
            _ => Clientes
        };
    }
}
