using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HBDrop.WebApp.Models;

/// <summary>
/// Represents a custom event (e.g., Father's Day, Mother's Day, Anniversary) associated with a contact
/// </summary>
public class CustomEvent
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
    /// Event name (e.g., "Father's Day", "Mother's Day", "Anniversary")
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string EventName { get; set; } = string.Empty;

    /// <summary>
    /// Event month (1-12)
    /// </summary>
    [Required]
    [Range(1, 12)]
    public int EventMonth { get; set; }

    /// <summary>
    /// Event day (1-31)
    /// </summary>
    [Required]
    [Range(1, 31)]
    public int EventDay { get; set; }

    /// <summary>
    /// Event year (optional - useful for anniversaries where year matters)
    /// </summary>
    [Range(1900, 2100)]
    public int? EventYear { get; set; }

    /// <summary>
    /// Additional notes about this event (used for AI message generation)
    /// </summary>
    [MaxLength(1000)]
    public string? Notes { get; set; }

    /// <summary>
    /// Custom message template for this event
    /// </summary>
    [MaxLength(1000)]
    public string? CustomMessage { get; set; }

    /// <summary>
    /// Optional: URL to a GIF image to send with the event message
    /// </summary>
    [MaxLength(1000)]
    public string? GifUrl { get; set; }

    /// <summary>
    /// Optional: Send to a group instead of individual
    /// </summary>
    public int? GroupId { get; set; }

    /// <summary>
    /// If true, this is a regional event that auto-calculates its date
    /// </summary>
    public bool IsRegionalEvent { get; set; } = false;

    /// <summary>
    /// The regional event type (e.g., "Father's Day", "Mother's Day")
    /// </summary>
    [MaxLength(100)]
    public string? RegionalEventType { get; set; }

    /// <summary>
    /// The country code for the regional event (e.g., "US", "IT", "IE")
    /// </summary>
    [MaxLength(10)]
    public string? RegionalEventCountryCode { get; set; }

    /// <summary>
    /// Timezone ID for this event (if different from contact's default)
    /// </summary>
    [MaxLength(100)]
    public string? TimeZoneId { get; set; }

    /// <summary>
    /// Hour (0-23) when to send the event message (if different from contact's default)
    /// </summary>
    [Range(0, 23)]
    public int? MessageHour { get; set; }

    /// <summary>
    /// When this event was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// When this event was last updated
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Whether this event is enabled
    /// </summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// Calculate the next occurrence of this event
    /// </summary>
    public DateTime GetNextOccurrence()
    {
        var now = DateTime.UtcNow;
        var currentYear = now.Year;
        
        // Try this year first
        try
        {
            var thisYear = new DateTime(currentYear, EventMonth, EventDay, 0, 0, 0, DateTimeKind.Utc);
            if (thisYear > now)
                return thisYear;
        }
        catch
        {
            // Invalid date for this year (e.g., Feb 29 in non-leap year)
        }
        
        // Try next year
        try
        {
            return new DateTime(currentYear + 1, EventMonth, EventDay, 0, 0, 0, DateTimeKind.Utc);
        }
        catch
        {
            // Invalid date for next year too, skip to year after
            return new DateTime(currentYear + 2, EventMonth, EventDay, 0, 0, 0, DateTimeKind.Utc);
        }
    }

    /// <summary>
    /// Get the display date for this event
    /// </summary>
    public string GetDateDisplay()
    {
        try
        {
            var date = new DateTime(EventYear ?? 2000, EventMonth, EventDay);
            return EventYear.HasValue 
                ? date.ToString("MMMM d, yyyy")
                : date.ToString("MMMM d");
        }
        catch
        {
            return $"{EventMonth}/{EventDay}" + (EventYear.HasValue ? $"/{EventYear}" : "");
        }
    }
}
