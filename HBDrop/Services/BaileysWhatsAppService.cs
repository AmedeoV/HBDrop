using System.Net.Http.Json;
using HBDrop.Models;

namespace HBDrop.Services;

public class BaileysWhatsAppService : IDisposable
{
    private readonly HttpClient _httpClient;
    private const string BaileysApiUrl = "http://localhost:3000";

    public BaileysWhatsAppService()
    {
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri(BaileysApiUrl),
            Timeout = TimeSpan.FromSeconds(30)
        };
    }

    public async Task<bool> IsConnectedAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("/health");
            if (!response.IsSuccessStatusCode) return false;

            var health = await response.Content.ReadFromJsonAsync<HealthResponse>();
            return health?.Connected ?? false;
        }
        catch
        {
            return false;
        }
    }

    public async Task<HealthResponse?> GetHealthAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("/health");
            if (!response.IsSuccessStatusCode) return null;

            return await response.Content.ReadFromJsonAsync<HealthResponse>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error checking health: {ex.Message}");
            return null;
        }
    }

    public async Task<(bool Success, string? QrCode)> GetQrCodeAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("/qr");
            var result = await response.Content.ReadFromJsonAsync<QrResponse>();
            
            return (result?.Success ?? false, result?.QrCode);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting QR code: {ex.Message}");
            return (false, null);
        }
    }

    public async Task<bool> SendMessageAsync(string phoneNumber, string message)
    {
        try
        {
            Console.WriteLine($"📤 Sending message to {phoneNumber}...");

            var payload = new
            {
                phone = phoneNumber,
                message = message
            };

            var response = await _httpClient.PostAsJsonAsync("/send", payload);
            
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"❌ Failed to send message: {error}");
                return false;
            }

            var result = await response.Content.ReadFromJsonAsync<SendResponse>();
            
            if (result?.Success ?? false)
            {
                Console.WriteLine($"✅ Message sent successfully!");
                return true;
            }

            Console.WriteLine($"❌ Failed to send message: {result?.Message}");
            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> LogoutAsync()
    {
        try
        {
            var response = await _httpClient.PostAsync("/logout", null);
            var result = await response.Content.ReadFromJsonAsync<SendResponse>();
            return result?.Success ?? false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error during logout: {ex.Message}");
            return false;
        }
    }

    public void Dispose()
    {
        _httpClient?.Dispose();
    }
}
