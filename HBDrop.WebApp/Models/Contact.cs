using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HBDrop.WebApp.Models;

/// <summary>
/// Represents a contact in the user's address book
/// </summary>
public class Contact
{
    [Key]
    public int Id { get; set; }

    /// <summary>
    /// Foreign key to the user who owns this contact
    /// </summary>
    [Required]
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// Navigation property to the owner
    /// </summary>
    [ForeignKey(nameof(UserId))]
    public ApplicationUser User { get; set; } = null!;

    /// <summary>
    /// Contact's full name
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Preferred name or nickname to use in birthday messages (optional)
    /// If not set, will use the first name from the full Name field
    /// </summary>
    [MaxLength(100)]
    public string? DisplayName { get; set; }

    /// <summary>
    /// WhatsApp phone number in international format (e.g., +1234567890)
    /// or WhatsApp group ID (e.g., 123456789-123456789@g.us)
    /// Optional - can be added later
    /// </summary>
    [MaxLength(100)]
    public string? PhoneNumber { get; set; }

    /// <summary>
    /// Indicates if this contact is a WhatsApp group
    /// </summary>
    public bool IsGroup { get; set; } = false;

    /// <summary>
    /// WhatsApp group ID if this is a group contact
    /// </summary>
    [MaxLength(100)]
    public string? GroupId { get; set; }

    /// <summary>
    /// Optional email address
    /// </summary>
    [MaxLength(200)]
    [EmailAddress]
    public string? Email { get; set; }

    /// <summary>
    /// Optional notes about the contact
    /// </summary>
    [MaxLength(1000)]
    public string? Notes { get; set; }

    /// <summary>
    /// IANA timezone identifier for the contact's location (e.g., "America/New_York", "Europe/Dublin", "Asia/Tokyo")
    /// Used to send birthday wishes at the correct local time. Defaults to UTC if not set.
    /// </summary>
    [MaxLength(100)]
    public string? TimeZoneId { get; set; }

    /// <summary>
    /// Hour of day (0-23) to send birthday message in contact's local timezone. Defaults to 9 AM.
    /// </summary>
    public int PreferredMessageHour { get; set; } = 9;

    /// <summary>
    /// When this contact was added
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Last update timestamp
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Navigation property to birthdays
    /// </summary>
    public ICollection<Birthday> Birthdays { get; set; } = new List<Birthday>();

    /// <summary>
    /// Navigation property to additional birthdays (e.g., kids, family members)
    /// </summary>
    public ICollection<AdditionalBirthday> AdditionalBirthdays { get; set; } = new List<AdditionalBirthday>();

    /// <summary>
    /// Navigation property to custom events (e.g., Father's Day, Mother's Day, Anniversary)
    /// </summary>
    public ICollection<CustomEvent> CustomEvents { get; set; } = new List<CustomEvent>();

    /// <summary>
    /// Navigation property to messages sent to this contact
    /// </summary>
    public ICollection<Message> ReceivedMessages { get; set; } = new List<Message>();
}
