using Microsoft.AspNetCore.Identity;

namespace HBDrop.WebApp.Models;

/// <summary>
/// Custom user class extending IdentityUser with additional properties
/// </summary>
public class ApplicationUser : IdentityUser
{
    /// <summary>
    /// User's full name
    /// </summary>
    public string? FullName { get; set; }

    /// <summary>
    /// When the user registered
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Last login timestamp
    /// </summary>
    public DateTime? LastLoginAt { get; set; }

    /// <summary>
    /// User's default timezone (IANA format, e.g., "Europe/Dublin")
    /// Used as default for all contacts unless overridden
    /// </summary>
    public string? DefaultTimeZoneId { get; set; }

    /// <summary>
    /// Default hour (0-23) to send birthday messages
    /// Used as default for all contacts unless overridden
    /// </summary>
    public int DefaultMessageHour { get; set; } = 9;

    /// <summary>
    /// Navigation property to user's WhatsApp session
    /// </summary>
    public WhatsAppSession? WhatsAppSession { get; set; }

    /// <summary>
    /// Navigation property to user's contacts
    /// </summary>
    public ICollection<Contact> Contacts { get; set; } = new List<Contact>();

    /// <summary>
    /// Navigation property to sent messages
    /// </summary>
    public ICollection<Message> SentMessages { get; set; } = new List<Message>();
}
