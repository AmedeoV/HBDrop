using Ical.Net;
using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;
using FuzzySharp;
using HBDrop.WebApp.Models;
using Microsoft.EntityFrameworkCore;
using HBDrop.WebApp.Data;

namespace HBDrop.WebApp.Services;

/// <summary>
/// Service for importing birthdays from calendar files (iCalendar/ICS format)
/// Compatible with Google Calendar, Apple Calendar, Outlook, and other calendar apps
/// </summary>
public class CalendarImportService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<CalendarImportService> _logger;

    public CalendarImportService(ApplicationDbContext dbContext, ILogger<CalendarImportService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    /// <summary>
    /// Parse an ICS calendar file and extract birthday events
    /// </summary>
    public async Task<CalendarImportResult> ParseCalendarFileAsync(Stream fileStream)
    {
        try
        {
            // Read the stream asynchronously into a string
            string calendarContent;
            using (var reader = new StreamReader(fileStream))
            {
                calendarContent = await reader.ReadToEndAsync();
            }

            // Parse the calendar from the string content
            var calendar = Calendar.Load(calendarContent);
            var result = new CalendarImportResult();

            foreach (var calendarEvent in calendar.Events)
            {
                // Check if this is a birthday event
                if (IsBirthdayEvent(calendarEvent))
                {
                    var birthdayEntry = ExtractBirthdayFromEvent(calendarEvent);
                    if (birthdayEntry != null)
                    {
                        result.BirthdayEntries.Add(birthdayEntry);
                    }
                }
            }

            result.Success = true;
            result.TotalEvents = calendar.Events.Count;
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing calendar file");
            return new CalendarImportResult
            {
                Success = false,
                ErrorMessage = $"Failed to parse calendar file: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// Match calendar birthday entries with existing contacts
    /// </summary>
    public async Task<List<BirthdayMatchResult>> MatchWithContactsAsync(
        List<CalendarBirthdayEntry> birthdayEntries,
        string userId)
    {
        var contacts = await _dbContext.Contacts
            .Where(c => c.UserId == userId)
            .ToListAsync();

        var matchResults = new List<BirthdayMatchResult>();

        foreach (var entry in birthdayEntries)
        {
            var matchResult = new BirthdayMatchResult
            {
                CalendarEntry = entry,
                PotentialMatches = new List<ContactMatch>()
            };

            // Try to find matching contacts
            foreach (var contact in contacts)
            {
                var match = CalculateMatch(entry, contact);
                if (match.MatchScore >= 60) // Only include matches with 60% or higher confidence
                {
                    matchResult.PotentialMatches.Add(match);
                }
            }

            // Sort matches by score (highest first)
            matchResult.PotentialMatches = matchResult.PotentialMatches
                .OrderByDescending(m => m.MatchScore)
                .ToList();

            // Auto-select the best match if it's very confident (90%+)
            if (matchResult.PotentialMatches.Any() && matchResult.PotentialMatches[0].MatchScore >= 90)
            {
                matchResult.SelectedContactId = matchResult.PotentialMatches[0].Contact.Id;
            }

            matchResults.Add(matchResult);
        }

        return matchResults;
    }

    /// <summary>
    /// Import confirmed birthday matches into the database
    /// </summary>
    public async Task<ImportResult> ImportBirthdaysAsync(List<BirthdayMatchResult> confirmedMatches, string userId)
    {
        var result = new ImportResult();

        foreach (var match in confirmedMatches)
        {
            try
            {
                if (match.SelectedContactId.HasValue && !match.CreateAsAdditionalBirthday)
                {
                    // Update existing contact's birthday
                    var contact = await _dbContext.Contacts
                        .Include(c => c.Birthdays)
                        .FirstOrDefaultAsync(c => c.Id == match.SelectedContactId.Value && c.UserId == userId);

                    if (contact != null)
                    {
                        // Check if birthday already exists
                        var existingBirthday = contact.Birthdays.FirstOrDefault();
                        if (existingBirthday != null)
                        {
                            existingBirthday.BirthDate = DateTime.SpecifyKind(match.CalendarEntry.BirthDate, DateTimeKind.Utc);
                            existingBirthday.UpdatedAt = DateTime.UtcNow;
                            result.Updated++;
                        }
                        else
                        {
                            var birthday = new Birthday
                            {
                                ContactId = contact.Id,
                                BirthDate = DateTime.SpecifyKind(match.CalendarEntry.BirthDate, DateTimeKind.Utc),
                                IsEnabled = true,
                                CreatedAt = DateTime.UtcNow,
                                UpdatedAt = DateTime.UtcNow
                            };
                            _dbContext.Birthdays.Add(birthday);
                            result.Imported++;
                        }
                    }
                }
                else if (match.SelectedContactId.HasValue && match.CreateAsAdditionalBirthday)
                {
                    // Create as additional birthday linked to existing contact
                    var contact = await _dbContext.Contacts
                        .Include(c => c.AdditionalBirthdays)
                        .FirstOrDefaultAsync(c => c.Id == match.SelectedContactId.Value && c.UserId == userId);

                    if (contact != null)
                    {
                        var additionalBirthday = new AdditionalBirthday
                        {
                            ContactId = contact.Id,
                            Name = match.CalendarEntry.Name,
                            BirthMonth = match.CalendarEntry.BirthDate.Month,
                            BirthDay = match.CalendarEntry.BirthDate.Day,
                            BirthYear = match.CalendarEntry.BirthDate.Year > 1900 ? match.CalendarEntry.BirthDate.Year : null,
                            Relationship = match.AdditionalBirthdayRelationship ?? "Family",
                            IsEnabled = true,
                            SendTo = "Contact",
                            SendToGroupId = null,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        };
                        _dbContext.AdditionalBirthdays.Add(additionalBirthday);
                        result.Imported++;
                    }
                }
                else if (match.CreateNewContact)
                {
                    // Create new contact with birthday
                    var newContact = new Contact
                    {
                        UserId = userId,
                        Name = match.CalendarEntry.Name,
                        PhoneNumber = match.NewContactPhoneNumber ?? string.Empty,
                        CreatedAt = DateTime.UtcNow,
                        IsGroup = false
                    };
                    _dbContext.Contacts.Add(newContact);
                    await _dbContext.SaveChangesAsync(); // Save to get contact ID

                    var birthday = new Birthday
                    {
                        ContactId = newContact.Id,
                        BirthDate = match.CalendarEntry.BirthDate,
                        IsEnabled = true
                    };
                    _dbContext.Birthdays.Add(birthday);
                    result.Imported++;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error importing birthday for {Name}", match.CalendarEntry.Name);
                result.Failed++;
            }
        }

        await _dbContext.SaveChangesAsync();
        result.Success = true;
        return result;
    }

    /// <summary>
    /// Search for contacts by name
    /// </summary>
    public async Task<List<Contact>> SearchContactsAsync(string searchTerm, string userId)
    {
        if (string.IsNullOrWhiteSpace(searchTerm) || searchTerm.Length < 2)
            return new List<Contact>();

        return await _dbContext.Contacts
            .Where(c => c.UserId == userId && 
                       (c.Name.ToLower().Contains(searchTerm.ToLower()) ||
                        c.PhoneNumber.Contains(searchTerm)))
            .OrderBy(c => c.Name)
            .Take(10)
            .ToListAsync();
    }

    /// <summary>
    /// Check if a calendar event represents a birthday
    /// </summary>
    private bool IsBirthdayEvent(CalendarEvent calendarEvent)
    {
        // Check various indicators that this is a birthday event
        var summary = calendarEvent.Summary?.ToLower() ?? string.Empty;
        var categories = calendarEvent.Categories?.Select(c => c.ToLower()).ToList() ?? new List<string>();

        // Check if the event is marked as a birthday category
        if (categories.Contains("birthday") || categories.Contains("birthdays"))
            return true;

        // Check if summary contains "birthday" or common birthday indicators
        if (summary.Contains("birthday") || summary.Contains("bday") || summary.Contains("🎂") || summary.Contains("🎉"))
            return true;

        // Google Calendar birthdays often have "Birthday" in the summary
        if (summary.EndsWith("'s birthday") || summary.Contains(" birthday"))
            return true;

        // Check for recurring yearly events (often birthdays)
        if (calendarEvent.RecurrenceRules.Any(r => r.Frequency == FrequencyType.Yearly))
        {
            // If it's yearly and has no end time (all-day event), likely a birthday
            if (calendarEvent.IsAllDay)
                return true;
        }

        return false;
    }

    /// <summary>
    /// Extract birthday information from a calendar event
    /// </summary>
    private CalendarBirthdayEntry? ExtractBirthdayFromEvent(CalendarEvent calendarEvent)
    {
        try
        {
            var summary = calendarEvent.Summary ?? string.Empty;
            
            // Extract name from summary (e.g., "John's Birthday" -> "John")
            var name = ExtractNameFromSummary(summary);
            
            if (string.IsNullOrWhiteSpace(name))
                return null;

            // Get the birth date - convert IDateTime to DateTime
            DateTime birthDate;
            if (calendarEvent.DtStart.Value is DateTime dt)
            {
                birthDate = dt;
            }
            else if (calendarEvent.DtStart.AsUtc != null)
            {
                birthDate = calendarEvent.DtStart.AsUtc;
            }
            else
            {
                // Fallback: try to parse the date
                birthDate = DateTime.Parse(calendarEvent.DtStart.ToString());
            }

            return new CalendarBirthdayEntry
            {
                Name = name,
                BirthDate = birthDate,
                OriginalSummary = summary,
                IsRecurring = calendarEvent.RecurrenceRules.Any()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extracting birthday from event: {Summary}", calendarEvent.Summary);
            return null;
        }
    }

    /// <summary>
    /// Extract person's name from calendar summary
    /// </summary>
    private string ExtractNameFromSummary(string summary)
    {
        // Remove common birthday indicators
        var name = summary
            .Replace("'s Birthday", "", StringComparison.OrdinalIgnoreCase)
            .Replace("'s birthday", "", StringComparison.OrdinalIgnoreCase)
            .Replace(" Birthday", "", StringComparison.OrdinalIgnoreCase)
            .Replace(" birthday", "", StringComparison.OrdinalIgnoreCase)
            .Replace("Birthday:", "", StringComparison.OrdinalIgnoreCase)
            .Replace("Birthday -", "", StringComparison.OrdinalIgnoreCase)
            .Replace("🎂", "")
            .Replace("🎉", "")
            .Replace("🎈", "")
            .Trim();

        return name;
    }

    /// <summary>
    /// Calculate match score between calendar entry and contact
    /// </summary>
    private ContactMatch CalculateMatch(CalendarBirthdayEntry entry, Contact contact)
    {
        var match = new ContactMatch
        {
            Contact = contact
        };

        // Use FuzzySharp for fuzzy string matching
        var nameScore = Fuzz.Ratio(entry.Name.ToLower(), contact.Name.ToLower());
        var partialScore = Fuzz.PartialRatio(entry.Name.ToLower(), contact.Name.ToLower());
        var tokenSortScore = Fuzz.TokenSortRatio(entry.Name.ToLower(), contact.Name.ToLower());

        // Take the best score from different matching algorithms
        match.MatchScore = Math.Max(nameScore, Math.Max(partialScore, tokenSortScore));

        // Build match reason
        if (match.MatchScore >= 90)
            match.MatchReason = "Exact or very close name match";
        else if (match.MatchScore >= 75)
            match.MatchReason = "Strong name similarity";
        else if (match.MatchScore >= 60)
            match.MatchReason = "Partial name match";
        else
            match.MatchReason = "Low confidence match";

        return match;
    }
}

/// <summary>
/// Result of parsing a calendar file
/// </summary>
public class CalendarImportResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public int TotalEvents { get; set; }
    public List<CalendarBirthdayEntry> BirthdayEntries { get; set; } = new();
}

/// <summary>
/// Represents a birthday entry extracted from a calendar
/// </summary>
public class CalendarBirthdayEntry
{
    public string Name { get; set; } = string.Empty;
    public DateTime BirthDate { get; set; }
    public string OriginalSummary { get; set; } = string.Empty;
    public bool IsRecurring { get; set; }
}

/// <summary>
/// Result of matching a calendar birthday with contacts
/// </summary>
public class BirthdayMatchResult
{
    public CalendarBirthdayEntry CalendarEntry { get; set; } = null!;
    public List<ContactMatch> PotentialMatches { get; set; } = new();
    public int? SelectedContactId { get; set; }
    public bool CreateNewContact { get; set; }
    public string? NewContactPhoneNumber { get; set; }
    public bool CreateAsAdditionalBirthday { get; set; }
    public string? AdditionalBirthdayRelationship { get; set; }
    public string? SearchTerm { get; set; }
    public bool ShowManualSearch { get; set; }
    public List<Contact>? SearchResults { get; set; }
}

/// <summary>
/// Represents a potential contact match
/// </summary>
public class ContactMatch
{
    public Contact Contact { get; set; } = null!;
    public int MatchScore { get; set; }
    public string MatchReason { get; set; } = string.Empty;
}

/// <summary>
/// Result of importing birthdays
/// </summary>
public class ImportResult
{
    public bool Success { get; set; }
    public int Imported { get; set; }
    public int Updated { get; set; }
    public int Failed { get; set; }
}
