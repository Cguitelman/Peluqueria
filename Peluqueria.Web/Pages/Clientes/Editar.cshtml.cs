using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Peluqueria.Web.Models;
using Peluqueria.Web.Services;

namespace Peluqueria.Web.Pages.Clientes;

public class EditarModel : PageModel
{
    private readonly ApiClient _api;
    public EditarModel(ApiClient api) => _api = api;

    [BindProperty] public int Id { get; set; }
    [BindProperty] public GuardarClienteDto Input { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var cliente = await _api.GetPorIdAsync<ClienteDto>($"api/clientes/{id}");
        if (cliente is null) return NotFound();
        Id = id;
        Input = new GuardarClienteDto { Nombre = cliente.Nombre, Apellido = cliente.Apellido, Email = cliente.Email, Telefono = cliente.Telefono };
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();
        try
        {
            await _api.PutAsync($"api/clientes/{Id}", Input);
            TempData["Success"] = "Cliente actualizado correctamente.";
            return RedirectToPage("Index");
        }
        catch (ApiException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return Page();
        }
    }
}
