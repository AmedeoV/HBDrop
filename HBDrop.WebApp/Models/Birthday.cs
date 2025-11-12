using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HBDrop.WebApp.Models;

/// <summary>
/// Represents a birthday reminder for a contact
/// </summary>
public class Birthday
{
    [Key]
    public int Id { get; set; }

    /// <summary>
    /// Foreign key to the contact
    /// </summary>
    [Required]
    public int ContactId { get; set; }

    /// <summary>
    /// Navigation property to the contact
    /// </summary>
    [ForeignKey(nameof(ContactId))]
    public Contact Contact { get; set; } = null!;

    /// <summary>
    /// Birth date (month and day are required, year is optional for age calculation)
    /// </summary>
    [Required]
    public DateTime BirthDate { get; set; }

    /// <summary>
    /// Custom message template for this birthday (optional)
    /// Use placeholders like {Name}, {Age} in the message
    /// </summary>
    [MaxLength(500)]
    public string? CustomMessage { get; set; }

    /// <summary>
    /// Optional: WhatsApp Group ID where the birthday message should be sent
    /// If set, message will be sent to this group instead of directly to the contact
    /// </summary>
    [MaxLength(100)]
    public string? SendToGroupId { get; set; }

    /// <summary>
    /// Whether to send birthday wishes automatically
    /// </summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// Last time a birthday message was sent
    /// </summary>
    public DateTime? LastSentAt { get; set; }

    /// <summary>
    /// When this birthday was added
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Last update timestamp
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Calculate age based on birth date and current date
    /// Returns null if birth year is not set
    /// </summary>
    public int? CalculateAge()
    {
        if (BirthDate.Year == 1) return null; // Year not set
        
        var today = DateTime.Today;
        var age = today.Year - BirthDate.Year;
        
        if (today.Month < BirthDate.Month || (today.Month == BirthDate.Month && today.Day < BirthDate.Day))
        {
            age--;
        }
        
        return age;
    }

    /// <summary>
    /// Check if today is this person's birthday
    /// </summary>
    public bool IsTodayTheirBirthday()
    {
        var today = DateTime.Today;
        return today.Month == BirthDate.Month && today.Day == BirthDate.Day;
    }

    /// <summary>
    /// Check if it's this person's birthday in their local timezone
    /// </summary>
    /// <param name="timeZoneId">IANA timezone identifier (e.g., "America/New_York")</param>
    public bool IsTodayTheirBirthdayInTimeZone(string? timeZoneId)
    {
        if (string.IsNullOrWhiteSpace(timeZoneId))
        {
            return IsTodayTheirBirthday();
        }

        try
        {
            var timeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
            var localDate = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZone).Date;
            return localDate.Month == BirthDate.Month && localDate.Day == BirthDate.Day;
        }
        catch
        {
            // If timezone is invalid, fall back to UTC
            return IsTodayTheirBirthday();
        }
    }

    /// <summary>
    /// Check if it's time to send the birthday message based on contact's timezone and preferred hour
    /// </summary>
    /// <param name="timeZoneId">IANA timezone identifier</param>
    /// <param name="preferredHour">Hour of day (0-23) to send the message</param>
    public bool IsTimeToSendMessage(string? timeZoneId, int preferredHour = 9)
    {
        if (!IsTodayTheirBirthdayInTimeZone(timeZoneId))
        {
            return false;
        }

        // Check if already sent today
        if (LastSentAt.HasValue)
        {
            try
            {
                var timeZone = string.IsNullOrWhiteSpace(timeZoneId) 
                    ? TimeZoneInfo.Utc 
                    : TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
                
                var lastSentInLocalTime = TimeZoneInfo.ConvertTimeFromUtc(LastSentAt.Value, timeZone);
                var nowInLocalTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZone);
                
                // If we already sent today in their timezone, don't send again
                if (lastSentInLocalTime.Date == nowInLocalTime.Date)
                {
                    return false;
                }
            }
            catch
            {
                // If timezone conversion fails, use simple UTC check
                if (LastSentAt.Value.Date == DateTime.UtcNow.Date)
                {
                    return false;
                }
            }
        }

        // Check if current hour in their timezone matches preferred hour
        try
        {
            var timeZone = string.IsNullOrWhiteSpace(timeZoneId) 
                ? TimeZoneInfo.Utc 
                : TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
            
            var nowInLocalTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZone);
            return nowInLocalTime.Hour == preferredHour;
        }
        catch
        {
            // If timezone is invalid, fall back to UTC
            return DateTime.UtcNow.Hour == preferredHour;
        }
    }

    /// <summary>
    /// Get the next occurrence of this birthday
    /// </summary>
    public DateTime GetNextBirthday()
    {
        var today = DateTime.Today;
        var thisYearBirthday = new DateTime(today.Year, BirthDate.Month, BirthDate.Day);
        
        return thisYearBirthday >= today ? thisYearBirthday : thisYearBirthday.AddYears(1);
    }
}
