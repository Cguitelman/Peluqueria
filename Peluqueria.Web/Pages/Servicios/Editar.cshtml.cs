using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Peluqueria.Web.Models;
using Peluqueria.Web.Services;

namespace Peluqueria.Web.Pages.Servicios;

[Authorize(Roles = "Administrador")]
public class EditarModel : PageModel
{
    private readonly ApiClient _api;
    public EditarModel(ApiClient api) => _api = api;

    [BindProperty] public int Id { get; set; }
    [BindProperty] public GuardarServicioDto Input { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var item = await _api.GetPorIdAsync<ServicioDto>($"api/servicios/{id}");
        if (item is null) return NotFound();
        Id = id;
        Input = new GuardarServicioDto { Nombre = item.Nombre, Descripcion = item.Descripcion, DuracionHoras = item.DuracionHoras, Precio = item.Precio };
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (Input.DuracionHoras * 2m != decimal.Truncate(Input.DuracionHoras * 2m))
            ModelState.AddModelError("Input.DuracionHoras", "Usá intervalos de media hora: 0,5; 1; 1,5; 2; etc.");
        if (!ModelState.IsValid) return Page();
        try { await _api.PutAsync($"api/servicios/{Id}", Input); TempData["Success"] = "Servicio actualizado correctamente.";
            return RedirectToPage("Index"); }
        catch (ApiException ex) { ModelState.AddModelError(string.Empty, ex.Message); return Page(); }
    }
}
