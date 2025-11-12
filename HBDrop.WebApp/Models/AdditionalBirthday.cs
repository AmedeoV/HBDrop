using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HBDrop.WebApp.Models;

/// <summary>
/// Represents an additional birthday (e.g., kids, family members) associated with a contact
/// </summary>
public class AdditionalBirthday
{
    [Key]
    public int Id { get; set; }

    /// <summary>
    /// Foreign key to the parent contact
    /// </summary>
    [Required]
    public int ContactId { get; set; }

    /// <summary>
    /// Navigation property to the parent contact
    /// </summary>
    [ForeignKey(nameof(ContactId))]
    public Contact Contact { get; set; } = null!;

    /// <summary>
    /// Name of the person (e.g., "John's son Michael")
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Birth month (1-12)
    /// </summary>
    [Required]
    [Range(1, 12)]
    public int BirthMonth { get; set; }

    /// <summary>
    /// Birth day (1-31)
    /// </summary>
    [Required]
    [Range(1, 31)]
    public int BirthDay { get; set; }

    /// <summary>
    /// Birth year (optional)
    /// </summary>
    [Range(1900, 2100)]
    public int? BirthYear { get; set; }

    /// <summary>
    /// Relationship to the contact (e.g., "Son", "Daughter", "Spouse")
    /// </summary>
    [MaxLength(50)]
    public string? Relationship { get; set; }

    /// <summary>
    /// Custom birthday message template (optional)
    /// Uses {Name} as placeholder for the birthday person's name
    /// </summary>
    [MaxLength(500)]
    public string? CustomMessage { get; set; }

    /// <summary>
    /// Optional: URL to a GIF image to send with the birthday message
    /// </summary>
    [MaxLength(1000)]
    public string? GifUrl { get; set; }

    /// <summary>
    /// Where to send the birthday message: "Contact" (to the parent contact) or "Group" (to a WhatsApp group)
    /// </summary>
    [Required]
    [MaxLength(20)]
    public string SendTo { get; set; } = "Contact"; // "Contact" or "Group"

    /// <summary>
    /// If SendTo is "Group", this is the group contact ID
    /// </summary>
    public int? SendToGroupId { get; set; }

    /// <summary>
    /// Navigation property to the group contact (if sending to a group)
    /// </summary>
    [ForeignKey(nameof(SendToGroupId))]
    public Contact? SendToGroup { get; set; }

    /// <summary>
    /// Timezone ID for this birthday (if different from contact's default)
    /// </summary>
    [MaxLength(100)]
    public string? TimeZoneId { get; set; }

    /// <summary>
    /// Hour (0-23) when to send the birthday message (if different from contact's default)
    /// </summary>
    [Range(0, 23)]
    public int? MessageHour { get; set; }

    /// <summary>
    /// Whether this birthday is enabled for automated messages
    /// </summary>
    public bool IsEnabled { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
