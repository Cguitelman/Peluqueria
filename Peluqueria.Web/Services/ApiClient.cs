using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Peluqueria.Web.Models;

namespace Peluqueria.Web.Services;

public class ApiClient
{
    private readonly HttpClient _http;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly JsonSerializerOptions _jsonOptions;

    public ApiClient(HttpClient http, IHttpContextAccessor httpContextAccessor)
    {
        _http = http;
        _httpContextAccessor = httpContextAccessor;
        _jsonOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        _jsonOptions.Converters.Add(new JsonStringEnumConverter());
    }

    public async Task<LoginResponseDto> LoginAsync(LoginRequestDto dto)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "api/auth/login")
        {
            Content = JsonContent.Create(dto, options: _jsonOptions)
        };
        using var response = await EnviarAsync(request);
        await ValidarRespuestaAsync(response);
        return (await response.Content.ReadFromJsonAsync<LoginResponseDto>(_jsonOptions))!;
    }

    public async Task LogoutAsync()
    {
        using var request = CrearRequest(HttpMethod.Post, "api/auth/logout");
        using var response = await EnviarAsync(request);
        if (response.StatusCode != System.Net.HttpStatusCode.Unauthorized)
            await ValidarRespuestaAsync(response);
    }

    public async Task<List<T>> GetAsync<T>(string url)
    {
        using var request = CrearRequest(HttpMethod.Get, url);
        using var response = await EnviarAsync(request);
        await ValidarRespuestaAsync(response);
        return await response.Content.ReadFromJsonAsync<List<T>>(_jsonOptions) ?? new List<T>();
    }

    public async Task<T?> GetPorIdAsync<T>(string url)
    {
        using var request = CrearRequest(HttpMethod.Get, url);
        using var response = await EnviarAsync(request);
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            return default;
        await ValidarRespuestaAsync(response);
        return await response.Content.ReadFromJsonAsync<T>(_jsonOptions);
    }

    public async Task PostAsync(string url)
    {
        using var request = CrearRequest(HttpMethod.Post, url);
        using var response = await EnviarAsync(request);
        await ValidarRespuestaAsync(response);
    }

    public async Task PostAsync<T>(string url, T body)
    {
        using var request = CrearRequest(HttpMethod.Post, url, JsonContent.Create(body, options: _jsonOptions));
        using var response = await EnviarAsync(request);
        await ValidarRespuestaAsync(response);
    }

    public async Task PutAsync<T>(string url, T body)
    {
        using var request = CrearRequest(HttpMethod.Put, url, JsonContent.Create(body, options: _jsonOptions));
        using var response = await EnviarAsync(request);
        await ValidarRespuestaAsync(response);
    }

    public async Task DeleteAsync(string url)
    {
        using var request = CrearRequest(HttpMethod.Delete, url);
        using var response = await EnviarAsync(request);
        await ValidarRespuestaAsync(response);
    }

    private HttpRequestMessage CrearRequest(HttpMethod method, string url, HttpContent? content = null)
    {
        var request = new HttpRequestMessage(method, url) { Content = content };
        var token = _httpContextAccessor.HttpContext?.Session.GetString("ApiToken");
        if (!string.IsNullOrWhiteSpace(token))
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return request;
    }

    private async Task<HttpResponseMessage> EnviarAsync(HttpRequestMessage request)
    {
        try
        {
            return await _http.SendAsync(request);
        }
        catch (HttpRequestException)
        {
            throw new ApiException("No se pudo conectar con la API. Verificá que Peluqueria.Api esté ejecutándose.", 0);
        }
        catch (TaskCanceledException)
        {
            throw new ApiException("La API tardó demasiado en responder. Intentá nuevamente.", 0);
        }
    }

    private static async Task ValidarRespuestaAsync(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode)
            return;

        var texto = await response.Content.ReadAsStringAsync();
        var mensaje = $"La API respondió {(int)response.StatusCode}.";
        if (!string.IsNullOrWhiteSpace(texto))
        {
            try
            {
                using var json = JsonDocument.Parse(texto);
                if (json.RootElement.TryGetProperty("mensaje", out var m)) mensaje = m.GetString() ?? mensaje;
                else if (json.RootElement.TryGetProperty("detail", out var d)) mensaje = d.GetString() ?? mensaje;
                else if (json.RootElement.TryGetProperty("title", out var t)) mensaje = t.GetString() ?? mensaje;
            }
            catch (JsonException)
            {
                mensaje = texto;
            }
        }
        throw new ApiException(mensaje, (int)response.StatusCode);
    }
}
