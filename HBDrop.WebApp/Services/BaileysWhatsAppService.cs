using System.Net.Http.Json;
using Microsoft.AspNetCore.Identity;
using HBDrop.WebApp.Models;

namespace HBDrop.WebApp.Services;

/// <summary>
/// Service for communicating with the Baileys WhatsApp API
/// </summary>
public class BaileysWhatsAppService : IWhatsAppService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<BaileysWhatsAppService> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly UserManager<ApplicationUser> _userManager;
    private const string BaileysApiUrl = "http://baileys:3000"; // Use Docker service name

    public BaileysWhatsAppService(
        HttpClient httpClient, 
        ILogger<BaileysWhatsAppService> logger,
        IHttpContextAccessor httpContextAccessor,
        UserManager<ApplicationUser> userManager)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri(BaileysApiUrl);
        _httpClient.Timeout = TimeSpan.FromSeconds(30);
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
        _userManager = userManager;
    }

    /// <summary>
    /// Gets the current authenticated user's ID
    /// </summary>
    private async Task<string> GetCurrentUserIdAsync()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null || httpContext.User == null || !httpContext.User.Identity?.IsAuthenticated == true)
        {
            throw new UnauthorizedAccessException("User is not authenticated");
        }

        var user = await _userManager.GetUserAsync(httpContext.User);
        if (user == null)
        {
            throw new UnauthorizedAccessException("User not found");
        }

        return user.Id;
    }

    public async Task<bool> IsConnectedAsync()
    {
        try
        {
            var userId = await GetCurrentUserIdAsync();
            var response = await _httpClient.GetAsync($"/status/{userId}");
            if (!response.IsSuccessStatusCode) return false;

            var status = await response.Content.ReadFromJsonAsync<ConnectionStatusResponse>();
            return status?.IsConnected ?? false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking connection status");
            return false;
        }
    }

    public async Task<HealthResponse?> GetHealthAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("/health");
            if (!response.IsSuccessStatusCode) return null;

            var health = await response.Content.ReadFromJsonAsync<HealthResponse>();
            
            // Set message for compatibility
            if (health != null)
            {
                health.Message = $"{health.ActiveSessions} active sessions";
            }
            
            return health;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking health");
            return null;
        }
    }

    public async Task<(bool Success, string? QrCode)> GetQrCodeAsync()
    {
        try
        {
            var userId = await GetCurrentUserIdAsync();
            var response = await _httpClient.GetAsync($"/qr/{userId}");
            var result = await response.Content.ReadFromJsonAsync<QrResponse>();
            
            return (result?.Success ?? false, result?.QrCode);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting QR code");
            return (false, null);
        }
    }

    public async Task<string?> GetQRCodeAsync(string userId)
    {
        try
        {
            _logger.LogInformation("Requesting QR code for user {UserId}", userId);
            
            var response = await _httpClient.GetAsync($"/qr/{userId}");
            var result = await response.Content.ReadFromJsonAsync<QrResponse>();
            
            if (result?.Success ?? false)
            {
                _logger.LogInformation("QR code generated successfully for user {UserId}", userId);
                return result.QrCode;
            }

            _logger.LogWarning("Failed to generate QR code: {Message}", result?.Message);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting QR code for user {UserId}", userId);
            return null;
        }
    }

    public async Task<string?> GetPairingCodeAsync(string userId, string phoneNumber)
    {
        try
        {
            _logger.LogInformation("Requesting pairing code for user {UserId} with phone {Phone}", userId, phoneNumber);
            
            var requestBody = new { phoneNumber };
            var response = await _httpClient.PostAsJsonAsync($"/pairing-code/{userId}", requestBody);
            var result = await response.Content.ReadFromJsonAsync<PairingCodeResponse>();
            
            if (result?.Success ?? false)
            {
                _logger.LogInformation("Pairing code generated successfully for user {UserId}", userId);
                return result.PairingCode;
            }

            _logger.LogWarning("Failed to generate pairing code: {Message}", result?.Message);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting pairing code for user {UserId}", userId);
            return null;
        }
    }

    public async Task<ConnectionStatusResponse?> GetConnectionStatusAsync(string userId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/status/{userId}");
            if (!response.IsSuccessStatusCode)
            {
                return new ConnectionStatusResponse { IsConnected = false };
            }

            var result = await response.Content.ReadFromJsonAsync<ConnectionStatusResponse>();
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking connection status for user {UserId}", userId);
            return new ConnectionStatusResponse { IsConnected = false };
        }
    }

    public async Task<bool> DisconnectAsync(string userId)
    {
        try
        {
            _logger.LogInformation("Disconnecting session for user {UserId}", userId);
            
            var response = await _httpClient.PostAsync($"/logout/{userId}", null);
            var result = await response.Content.ReadFromJsonAsync<SendResponse>();
            
            bool success = result?.Success ?? false;
            if (success)
            {
                _logger.LogInformation("Session disconnected successfully for user {UserId}", userId);
            }
            
            return success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error disconnecting session for user {UserId}", userId);
            return false;
        }
    }

    public async Task<bool> SendMessageAsync(string phoneNumber, string message)
    {
        return await SendMessageAsync(phoneNumber, message, null);
    }

    public async Task<bool> SendMessageAsync(string phoneNumber, string message, string? gifUrl)
    {
        try
        {
            var userId = await GetCurrentUserIdAsync();
            _logger.LogInformation("User {UserId} sending message to {PhoneNumber}", userId, phoneNumber);

            var payload = new
            {
                phone = phoneNumber,
                message = message,
                gifUrl = gifUrl
            };

            var response = await _httpClient.PostAsJsonAsync($"/send/{userId}", payload);
            
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                _logger.LogError("Failed to send message: {Error}", error);
                return false;
            }

            var result = await response.Content.ReadFromJsonAsync<SendResponse>();
            
            if (result?.Success ?? false)
            {
                _logger.LogInformation("Message sent successfully to {PhoneNumber}", phoneNumber);
                return true;
            }

            _logger.LogError("Failed to send message: {Message}", result?.Message);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending message to {PhoneNumber}", phoneNumber);
            return false;
        }
    }

    /// <summary>
    /// Send a WhatsApp message for a specific user (for background jobs without HttpContext)
    /// </summary>
    public async Task<bool> SendMessageAsync(string userId, string phoneNumber, string message, string? gifUrl = null)
    {
        try
        {
            _logger.LogInformation("User {UserId} sending message to {PhoneNumber} (background job)", userId, phoneNumber);

            var payload = new
            {
                phone = phoneNumber,
                message = message,
                gifUrl = gifUrl
            };

            var response = await _httpClient.PostAsJsonAsync($"/send/{userId}", payload);
            
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                _logger.LogError("Failed to send message for user {UserId}: {Error}", userId, error);
                return false;
            }

            var result = await response.Content.ReadFromJsonAsync<SendResponse>();
            
            if (result?.Success ?? false)
            {
                _logger.LogInformation("Message sent successfully to {PhoneNumber} for user {UserId}", phoneNumber, userId);
                return true;
            }

            _logger.LogError("Failed to send message for user {UserId}: {Message}", userId, result?.Message);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending message to {PhoneNumber} for user {UserId}", phoneNumber, userId);
            return false;
        }
    }

    public async Task<List<WhatsAppGroup>> GetGroupsAsync()
    {
        try
        {
            var userId = await GetCurrentUserIdAsync();
            _logger.LogInformation("Fetching WhatsApp groups for user {UserId}", userId);

            var response = await _httpClient.GetAsync($"/groups/{userId}");
            
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                _logger.LogError("Failed to fetch groups: {Error}", error);
                return new List<WhatsAppGroup>();
            }

            var result = await response.Content.ReadFromJsonAsync<GroupsResponse>();
            
            if (result?.Success ?? false)
            {
                _logger.LogInformation("Successfully fetched {Count} groups", result.Groups?.Count ?? 0);
                return result.Groups ?? new List<WhatsAppGroup>();
            }

            _logger.LogError("Failed to fetch groups: {Message}", result?.Message);
            return new List<WhatsAppGroup>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching WhatsApp groups");
            return new List<WhatsAppGroup>();
        }
    }

    public async Task<List<WhatsAppContact>> GetContactsAsync()
    {
        try
        {
            var userId = await GetCurrentUserIdAsync();
            _logger.LogInformation("Fetching WhatsApp contacts for user {UserId}", userId);

            var response = await _httpClient.GetAsync($"/contacts/{userId}");
            
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                _logger.LogError("Failed to fetch contacts: {Error}", error);
                return new List<WhatsAppContact>();
            }

            var result = await response.Content.ReadFromJsonAsync<ContactsResponse>();
            
            if (result?.Success ?? false)
            {
                _logger.LogInformation("Successfully fetched {Count} contacts", result.Contacts?.Count ?? 0);
                return result.Contacts ?? new List<WhatsAppContact>();
            }

            _logger.LogError("Failed to fetch contacts: {Message}", result?.Message);
            return new List<WhatsAppContact>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching WhatsApp contacts");
            return new List<WhatsAppContact>();
        }
    }

    public async Task<bool> LogoutAsync()
    {
        try
        {
            var userId = await GetCurrentUserIdAsync();
            var response = await _httpClient.PostAsync($"/logout/{userId}", null);
            var result = await response.Content.ReadFromJsonAsync<SendResponse>();
            return result?.Success ?? false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during logout");
            return false;
        }
    }
}

// Response models
public class HealthResponse
{
    public string? Status { get; set; }
    public int ActiveSessions { get; set; }
    public List<string>? Users { get; set; }
    
    // Compatibility properties
    public bool Connected => ActiveSessions > 0;
    public string? Message { get; set; }
}

public class QrResponse
{
    public bool Success { get; set; }
    public string? QrCode { get; set; }
    public string? QrImage { get; set; }
    public string? Message { get; set; }
    public bool? Connected { get; set; }
}

public class PairingCodeResponse
{
    public bool Success { get; set; }
    public string? PairingCode { get; set; }
    public string? Message { get; set; }
    public bool? Connected { get; set; }
}

public class SendResponse
{
    public bool Success { get; set; }
    public string? Message { get; set; }
}
