using System.Net.Http.Json;
using System.Text.Json;
using VisorDatosSIG.Application.DTOs;

namespace VisorDatosSIG.Mobile.Services;

public class ApiService
{
    private readonly HttpClient _httpClient;
    
    // URL Pública para acceder por 4G/5G desde cualquier lado
    private readonly string _baseUrl = "https://eleven-sides-poke.loca.lt/api";

    public ApiService()
    {
        // Ignorar errores de certificado SSL en desarrollo local
        var handler = new HttpsClientHandlerService();
        _httpClient = new HttpClient(handler.GetPlatformMessageHandler());
        _httpClient.BaseAddress = new Uri(_baseUrl);
        _httpClient.DefaultRequestHeaders.Add("Bypass-Tunnel-Reminder", "true");
    }

    public async Task<string?> LoginAsync(string username, string password)
    {
        try
        {
            var request = new LoginMobileRequest
            {
                Username = username,
                Password = password
            };

            var response = await _httpClient.PostAsJsonAsync($"{_baseUrl}/mobile/login", request);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<JsonElement>(content);
                return result.GetProperty("token").GetString();
            }

            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error en Login: {ex.Message}");
            return null;
        }
    }

    public async Task<string?> GetManzanasGeoJsonAsync(string token)
    {
        try
        {
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            var response = await _httpClient.GetAsync($"{_baseUrl}/geodata/manzanas");

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsStringAsync();
            }
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error en GetManzanas: {ex.Message}");
            return null;
        }
    }
}

// Clase auxiliar para ignorar el SSL (Solo para desarrollo)
public class HttpsClientHandlerService
{
    public HttpMessageHandler GetPlatformMessageHandler()
    {
#if ANDROID
        var handler = new Xamarin.Android.Net.AndroidMessageHandler();
        handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) =>
        {
            return true; 
        };
        return handler;
#elif WINDOWS
        return new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (message, cert, chain, errors) =>
            {
                return true;
            }
        };
#else
        return new HttpClientHandler();
#endif
    }
}
