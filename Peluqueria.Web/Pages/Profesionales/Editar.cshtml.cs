using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Peluqueria.Web.Models;
using Peluqueria.Web.Services;

namespace Peluqueria.Web.Pages.Profesionales;

[Authorize(Roles = "Administrador")]
public class EditarModel : PageModel
{
    private readonly ApiClient _api;
    public EditarModel(ApiClient api) => _api = api;

    [BindProperty] public int Id { get; set; }
    [BindProperty] public GuardarProfesionalDto Input { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var item = await _api.GetPorIdAsync<ProfesionalDto>($"api/profesionales/{id}");
        if (item is null) return NotFound();
        Id = id;
        Input = new GuardarProfesionalDto { Nombre = item.Nombre, Apellido = item.Apellido, Email = item.Email, Especialidad = item.Especialidad };
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();
        try { await _api.PutAsync($"api/profesionales/{Id}", Input); TempData["Success"] = "Profesional actualizado correctamente.";
            return RedirectToPage("Index"); }
        catch (ApiException ex) { ModelState.AddModelError(string.Empty, ex.Message); return Page(); }
    }
}
