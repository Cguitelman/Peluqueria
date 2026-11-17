using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Peluqueria.Web.Services;

namespace Peluqueria.Web.Pages.Cuenta;

public class LogoutModel : PageModel
{
    private readonly ApiClient _api;
    public LogoutModel(ApiClient api) => _api = api;

    public async Task<IActionResult> OnPostAsync()
    {
        try { await _api.LogoutAsync(); } catch { }
        HttpContext.Session.Clear();
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToPage("/Cuenta/Login");
    }
}
