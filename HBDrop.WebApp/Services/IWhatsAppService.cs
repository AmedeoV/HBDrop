namespace HBDrop.WebApp.Services;

/// <summary>
/// Interface for WhatsApp service operations
/// </summary>
public interface IWhatsAppService
{
    /// <summary>
    /// Check if WhatsApp is connected
    /// </summary>
    Task<bool> IsConnectedAsync();

    /// <summary>
    /// Get health status from Baileys service
    /// </summary>
    Task<HealthResponse?> GetHealthAsync();

    /// <summary>
    /// Request a QR code for connecting WhatsApp
    /// </summary>
    Task<string?> GetQRCodeAsync(string userId);

    /// <summary>
    /// Request a pairing code for connecting WhatsApp (alternative to QR code)
    /// </summary>
    Task<string?> GetPairingCodeAsync(string userId, string phoneNumber);

    /// <summary>
    /// Get connection status for a specific user session
    /// </summary>
    Task<ConnectionStatusResponse?> GetConnectionStatusAsync(string userId);

    /// <summary>
    /// Send a WhatsApp message (uses current authenticated user from HttpContext)
    /// </summary>
    Task<bool> SendMessageAsync(string phoneNumber, string message);

    /// <summary>
    /// Send a WhatsApp message for a specific user (for background jobs)
    /// </summary>
    Task<bool> SendMessageAsync(string userId, string phoneNumber, string message);

    /// <summary>
    /// Get list of WhatsApp groups
    /// </summary>
    Task<List<WhatsAppGroup>> GetGroupsAsync();

    /// <summary>
    /// Get list of WhatsApp contacts
    /// </summary>
    Task<List<WhatsAppContact>> GetContactsAsync();

    /// <summary>
    /// Disconnect WhatsApp session
    /// </summary>
    Task<bool> DisconnectAsync(string userId);
}

/// <summary>
/// Response model for connection status
/// </summary>
public class ConnectionStatusResponse
{
    public bool IsConnected { get; set; }
    public string? PhoneNumber { get; set; }
    public string? SessionId { get; set; }
    public string? Message { get; set; }
}

/// <summary>
/// Model for WhatsApp group
/// </summary>
public class WhatsAppGroup
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public int Participants { get; set; }
    public string? Owner { get; set; }
    public string? Description { get; set; }
    public long? CreatedAt { get; set; }
    public long? LastMessageTime { get; set; }
    public bool IsAnnounce { get; set; }
}

/// <summary>
/// Response model for groups list
/// </summary>
public class GroupsResponse
{
    public bool Success { get; set; }
    public List<WhatsAppGroup> Groups { get; set; } = new();
    public int Count { get; set; }
    public string? Message { get; set; }
}

/// <summary>
/// Model for WhatsApp contact
/// </summary>
public class WhatsAppContact
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string Phone { get; set; } = "";
    public string? Notify { get; set; }
    public string? VerifiedName { get; set; }
    public string? ImgUrl { get; set; }
}

/// <summary>
/// Response model for contacts list
/// </summary>
public class ContactsResponse
{
    public bool Success { get; set; }
    public List<WhatsAppContact> Contacts { get; set; } = new();
    public int Count { get; set; }
    public string? Message { get; set; }
}
