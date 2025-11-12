using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HBDrop.WebApp.Models;

/// <summary>
/// Represents a WhatsApp session for a user
/// Stores encrypted session data for maintaining WhatsApp connection
/// </summary>
public class WhatsAppSession
{
    [Key]
    public int Id { get; set; }

    /// <summary>
    /// Foreign key to the user (one-to-one relationship)
    /// </summary>
    [Required]
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// Navigation property to the user
    /// </summary>
    [ForeignKey(nameof(UserId))]
    public ApplicationUser User { get; set; } = null!;

    /// <summary>
    /// WhatsApp phone number associated with this session
    /// </summary>
    [Required]
    [MaxLength(20)]
    public string PhoneNumber { get; set; } = string.Empty;

    /// <summary>
    /// Encrypted session data (JSON containing auth credentials)
    /// This is encrypted using AES-256 for security
    /// </summary>
    [Required]
    public string EncryptedSessionData { get; set; } = string.Empty;

    /// <summary>
    /// Initialization vector for AES encryption
    /// </summary>
    [Required]
    public string EncryptionIV { get; set; } = string.Empty;

    /// <summary>
    /// Whether this session is currently active and connected
    /// </summary>
    public bool IsActive { get; set; } = false;

    /// <summary>
    /// Last time the connection was verified
    /// </summary>
    public DateTime? LastVerifiedAt { get; set; }

    /// <summary>
    /// QR code data URL (base64 encoded image) for initial connection
    /// This is temporary and cleared once connection is established
    /// </summary>
    public string? QrCode { get; set; }

    /// <summary>
    /// When the QR code expires (usually 60 seconds)
    /// </summary>
    public DateTime? QrCodeExpiresAt { get; set; }

    /// <summary>
    /// When this session was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Last update timestamp
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// When this session was last used to send a message
    /// </summary>
    public DateTime? LastUsedAt { get; set; }

    /// <summary>
    /// Check if the QR code is still valid
    /// </summary>
    public bool IsQrCodeValid()
    {
        return QrCode != null 
            && QrCodeExpiresAt.HasValue 
            && QrCodeExpiresAt.Value > DateTime.UtcNow;
    }

    /// <summary>
    /// Check if the session needs re-authentication (inactive for more than 30 days)
    /// </summary>
    public bool NeedsReauthentication()
    {
        if (!IsActive) return true;
        
        var lastActivity = LastUsedAt ?? LastVerifiedAt ?? UpdatedAt ?? CreatedAt;
        return DateTime.UtcNow - lastActivity > TimeSpan.FromDays(30);
    }
}
