namespace HBDrop.WebApp.Services;

using HBDrop.WebApp.Models;

/// <summary>
/// Service for managing regional event definitions (Father's Day, Mother's Day, etc.)
/// </summary>
public class RegionalEventsService
{
    private readonly List<RegionalEventDefinition> _eventDefinitions;

    public RegionalEventsService()
    {
        _eventDefinitions = InitializeEventDefinitions();
    }

    /// <summary>
    /// Get all available event types
    /// </summary>
    public List<string> GetEventTypes()
    {
        return _eventDefinitions
            .Select(e => e.EventName)
            .Distinct()
            .OrderBy(e => e)
            .ToList();
    }

    /// <summary>
    /// Get all countries/regions that have a specific event
    /// </summary>
    public List<(string Country, string CountryCode)> GetCountriesForEvent(string eventName)
    {
        return _eventDefinitions
            .Where(e => e.EventName == eventName)
            .Select(e => (e.Country, e.CountryCode))
            .OrderBy(c => c.Country)
            .ToList();
    }

    /// <summary>
    /// Get the event definition for a specific event and country
    /// </summary>
    public RegionalEventDefinition? GetEventDefinition(string eventName, string countryCode)
    {
        return _eventDefinitions.FirstOrDefault(e => 
            e.EventName == eventName && e.CountryCode == countryCode);
    }

    /// <summary>
    /// Calculate the date for an event in a specific country for a given year
    /// </summary>
    public DateTime? CalculateEventDate(string eventName, string countryCode, int year)
    {
        var definition = GetEventDefinition(eventName, countryCode);
        return definition?.CalculateDateForYear(year);
    }

    /// <summary>
    /// Get event definition by country code (used for auto-detection from timezone)
    /// </summary>
    public List<RegionalEventDefinition> GetEventsByCountryCode(string countryCode)
    {
        return _eventDefinitions
            .Where(e => e.CountryCode == countryCode)
            .ToList();
    }

    /// <summary>
    /// Try to map a timezone to a country code
    /// </summary>
    public string? GetCountryCodeFromTimezone(string timezoneId)
    {
        if (string.IsNullOrWhiteSpace(timezoneId)) return null;

        var timezoneToCountry = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            // Europe
            { "Europe/Dublin", "IE" },
            { "Europe/London", "GB" },
            { "Europe/Paris", "FR" },
            { "Europe/Berlin", "DE" },
            { "Europe/Madrid", "ES" },
            { "Europe/Rome", "IT" },
            { "Europe/Lisbon", "PT" },
            { "Europe/Amsterdam", "NL" },
            { "Europe/Brussels", "BE" },
            { "Europe/Stockholm", "SE" },
            { "Europe/Oslo", "NO" },
            { "Europe/Copenhagen", "DK" },
            { "Europe/Helsinki", "FI" },
            { "Europe/Vienna", "AT" },
            { "Europe/Zurich", "CH" },
            { "Europe/Warsaw", "PL" },
            { "Europe/Athens", "GR" },
            
            // Americas
            { "America/New_York", "US" },
            { "America/Chicago", "US" },
            { "America/Denver", "US" },
            { "America/Los_Angeles", "US" },
            { "America/Toronto", "CA" },
            { "America/Vancouver", "CA" },
            { "America/Mexico_City", "MX" },
            { "America/Sao_Paulo", "BR" },
            { "America/Argentina/Buenos_Aires", "AR" },
            
            // Asia-Pacific
            { "Asia/Tokyo", "JP" },
            { "Asia/Shanghai", "CN" },
            { "Asia/Hong_Kong", "HK" },
            { "Asia/Singapore", "SG" },
            { "Asia/Seoul", "KR" },
            { "Asia/Dubai", "AE" },
            { "Asia/Kolkata", "IN" },
            { "Asia/Bangkok", "TH" },
            { "Asia/Jakarta", "ID" },
            { "Australia/Sydney", "AU" },
            { "Australia/Melbourne", "AU" },
            { "Pacific/Auckland", "NZ" }
        };

        return timezoneToCountry.TryGetValue(timezoneId, out var countryCode) ? countryCode : null;
    }

    private List<RegionalEventDefinition> InitializeEventDefinitions()
    {
        return new List<RegionalEventDefinition>
        {
            // ========== MOTHER'S DAY ==========
            
            // Second Sunday of May (Most common - USA, Canada, Australia, etc.)
            new RegionalEventDefinition
            {
                EventName = "Mother's Day",
                Country = "🇺🇸 United States",
                CountryCode = "US",
                DateType = EventDateType.Floating,
                FloatingMonth = 5,
                FloatingDayOfWeek = DayOfWeek.Sunday,
                FloatingWeekNumber = 2
            },
            new RegionalEventDefinition
            {
                EventName = "Mother's Day",
                Country = "🇨🇦 Canada",
                CountryCode = "CA",
                DateType = EventDateType.Floating,
                FloatingMonth = 5,
                FloatingDayOfWeek = DayOfWeek.Sunday,
                FloatingWeekNumber = 2
            },
            new RegionalEventDefinition
            {
                EventName = "Mother's Day",
                Country = "🇦🇺 Australia",
                CountryCode = "AU",
                DateType = EventDateType.Floating,
                FloatingMonth = 5,
                FloatingDayOfWeek = DayOfWeek.Sunday,
                FloatingWeekNumber = 2
            },
            new RegionalEventDefinition
            {
                EventName = "Mother's Day",
                Country = "🇳🇿 New Zealand",
                CountryCode = "NZ",
                DateType = EventDateType.Floating,
                FloatingMonth = 5,
                FloatingDayOfWeek = DayOfWeek.Sunday,
                FloatingWeekNumber = 2
            },
            new RegionalEventDefinition
            {
                EventName = "Mother's Day",
                Country = "🇧🇷 Brazil",
                CountryCode = "BR",
                DateType = EventDateType.Floating,
                FloatingMonth = 5,
                FloatingDayOfWeek = DayOfWeek.Sunday,
                FloatingWeekNumber = 2
            },
            new RegionalEventDefinition
            {
                EventName = "Mother's Day",
                Country = "🇯🇵 Japan",
                CountryCode = "JP",
                DateType = EventDateType.Floating,
                FloatingMonth = 5,
                FloatingDayOfWeek = DayOfWeek.Sunday,
                FloatingWeekNumber = 2
            },
            
            // Fourth Sunday of Lent (UK, Ireland)
            new RegionalEventDefinition
            {
                EventName = "Mother's Day",
                Country = "🇬🇧 United Kingdom",
                CountryCode = "GB",
                DateType = EventDateType.Floating,
                FloatingMonth = 3, // Approximation - varies by Easter
                FloatingDayOfWeek = DayOfWeek.Sunday,
                FloatingWeekNumber = 4
            },
            new RegionalEventDefinition
            {
                EventName = "Mother's Day",
                Country = "🇮🇪 Ireland",
                CountryCode = "IE",
                DateType = EventDateType.Floating,
                FloatingMonth = 3, // Approximation - varies by Easter
                FloatingDayOfWeek = DayOfWeek.Sunday,
                FloatingWeekNumber = 4
            },
            
            // Last Sunday of May (France)
            new RegionalEventDefinition
            {
                EventName = "Mother's Day",
                Country = "🇫🇷 France",
                CountryCode = "FR",
                DateType = EventDateType.Floating,
                FloatingMonth = 5,
                FloatingDayOfWeek = DayOfWeek.Sunday,
                FloatingWeekNumber = -1 // Last Sunday
            },
            
            // First Sunday of May (Spain, Portugal)
            new RegionalEventDefinition
            {
                EventName = "Mother's Day",
                Country = "🇪🇸 Spain",
                CountryCode = "ES",
                DateType = EventDateType.Floating,
                FloatingMonth = 5,
                FloatingDayOfWeek = DayOfWeek.Sunday,
                FloatingWeekNumber = 1
            },
            new RegionalEventDefinition
            {
                EventName = "Mother's Day",
                Country = "🇵🇹 Portugal",
                CountryCode = "PT",
                DateType = EventDateType.Floating,
                FloatingMonth = 5,
                FloatingDayOfWeek = DayOfWeek.Sunday,
                FloatingWeekNumber = 1
            },
            
            // ========== FATHER'S DAY ==========
            
            // Third Sunday of June (Most common - USA, UK, Canada, etc.)
            new RegionalEventDefinition
            {
                EventName = "Father's Day",
                Country = "🇺🇸 United States",
                CountryCode = "US",
                DateType = EventDateType.Floating,
                FloatingMonth = 6,
                FloatingDayOfWeek = DayOfWeek.Sunday,
                FloatingWeekNumber = 3
            },
            new RegionalEventDefinition
            {
                EventName = "Father's Day",
                Country = "🇬🇧 United Kingdom",
                CountryCode = "GB",
                DateType = EventDateType.Floating,
                FloatingMonth = 6,
                FloatingDayOfWeek = DayOfWeek.Sunday,
                FloatingWeekNumber = 3
            },
            new RegionalEventDefinition
            {
                EventName = "Father's Day",
                Country = "🇮🇪 Ireland",
                CountryCode = "IE",
                DateType = EventDateType.Floating,
                FloatingMonth = 6,
                FloatingDayOfWeek = DayOfWeek.Sunday,
                FloatingWeekNumber = 3
            },
            new RegionalEventDefinition
            {
                EventName = "Father's Day",
                Country = "🇨🇦 Canada",
                CountryCode = "CA",
                DateType = EventDateType.Floating,
                FloatingMonth = 6,
                FloatingDayOfWeek = DayOfWeek.Sunday,
                FloatingWeekNumber = 3
            },
            new RegionalEventDefinition
            {
                EventName = "Father's Day",
                Country = "🇫🇷 France",
                CountryCode = "FR",
                DateType = EventDateType.Floating,
                FloatingMonth = 6,
                FloatingDayOfWeek = DayOfWeek.Sunday,
                FloatingWeekNumber = 3
            },
            new RegionalEventDefinition
            {
                EventName = "Father's Day",
                Country = "🇯🇵 Japan",
                CountryCode = "JP",
                DateType = EventDateType.Floating,
                FloatingMonth = 6,
                FloatingDayOfWeek = DayOfWeek.Sunday,
                FloatingWeekNumber = 3
            },
            new RegionalEventDefinition
            {
                EventName = "Father's Day",
                Country = "🇨🇳 China",
                CountryCode = "CN",
                DateType = EventDateType.Floating,
                FloatingMonth = 6,
                FloatingDayOfWeek = DayOfWeek.Sunday,
                FloatingWeekNumber = 3
            },
            new RegionalEventDefinition
            {
                EventName = "Father's Day",
                Country = "🇮🇳 India",
                CountryCode = "IN",
                DateType = EventDateType.Floating,
                FloatingMonth = 6,
                FloatingDayOfWeek = DayOfWeek.Sunday,
                FloatingWeekNumber = 3
            },
            
            // March 19 (Italy, Spain, Portugal - Saint Joseph's Day)
            new RegionalEventDefinition
            {
                EventName = "Father's Day",
                Country = "🇮🇹 Italy",
                CountryCode = "IT",
                DateType = EventDateType.Fixed,
                FixedMonth = 3,
                FixedDay = 19
            },
            new RegionalEventDefinition
            {
                EventName = "Father's Day",
                Country = "🇪🇸 Spain",
                CountryCode = "ES",
                DateType = EventDateType.Fixed,
                FixedMonth = 3,
                FixedDay = 19
            },
            new RegionalEventDefinition
            {
                EventName = "Father's Day",
                Country = "🇵🇹 Portugal",
                CountryCode = "PT",
                DateType = EventDateType.Fixed,
                FixedMonth = 3,
                FixedDay = 19
            },
            
            // First Sunday of September (Australia, New Zealand)
            new RegionalEventDefinition
            {
                EventName = "Father's Day",
                Country = "🇦🇺 Australia",
                CountryCode = "AU",
                DateType = EventDateType.Floating,
                FloatingMonth = 9,
                FloatingDayOfWeek = DayOfWeek.Sunday,
                FloatingWeekNumber = 1
            },
            new RegionalEventDefinition
            {
                EventName = "Father's Day",
                Country = "🇳🇿 New Zealand",
                CountryCode = "NZ",
                DateType = EventDateType.Floating,
                FloatingMonth = 9,
                FloatingDayOfWeek = DayOfWeek.Sunday,
                FloatingWeekNumber = 1
            },
            
            // Second Sunday of August (Brazil)
            new RegionalEventDefinition
            {
                EventName = "Father's Day",
                Country = "🇧🇷 Brazil",
                CountryCode = "BR",
                DateType = EventDateType.Floating,
                FloatingMonth = 8,
                FloatingDayOfWeek = DayOfWeek.Sunday,
                FloatingWeekNumber = 2
            },
            
            // ========== VALENTINE'S DAY ==========
            
            // February 14 (International)
            new RegionalEventDefinition
            {
                EventName = "Valentine's Day",
                Country = "🌍 International",
                CountryCode = "INT",
                DateType = EventDateType.Fixed,
                FixedMonth = 2,
                FixedDay = 14
            },
            
            // ========== CHRISTMAS ==========
            
            // December 25 (International)
            new RegionalEventDefinition
            {
                EventName = "Christmas",
                Country = "🌍 International",
                CountryCode = "INT",
                DateType = EventDateType.Fixed,
                FixedMonth = 12,
                FixedDay = 25
            },
            
            // ========== NEW YEAR'S DAY ==========
            
            // January 1 (International)
            new RegionalEventDefinition
            {
                EventName = "New Year's Day",
                Country = "🌍 International",
                CountryCode = "INT",
                DateType = EventDateType.Fixed,
                FixedMonth = 1,
                FixedDay = 1
            },
            
            // ========== THANKSGIVING ==========
            
            // Fourth Thursday of November (USA)
            new RegionalEventDefinition
            {
                EventName = "Thanksgiving",
                Country = "🇺🇸 United States",
                CountryCode = "US",
                DateType = EventDateType.Floating,
                FloatingMonth = 11,
                FloatingDayOfWeek = DayOfWeek.Thursday,
                FloatingWeekNumber = 4
            },
            
            // Second Monday of October (Canada)
            new RegionalEventDefinition
            {
                EventName = "Thanksgiving",
                Country = "🇨🇦 Canada",
                CountryCode = "CA",
                DateType = EventDateType.Floating,
                FloatingMonth = 10,
                FloatingDayOfWeek = DayOfWeek.Monday,
                FloatingWeekNumber = 2
            },
            
            // ========== GRANDPARENTS' DAY ==========
            
            // First Sunday after Labor Day (USA)
            new RegionalEventDefinition
            {
                EventName = "Grandparents' Day",
                Country = "🇺🇸 United States",
                CountryCode = "US",
                DateType = EventDateType.Floating,
                FloatingMonth = 9,
                FloatingDayOfWeek = DayOfWeek.Sunday,
                FloatingWeekNumber = 2 // Approximation
            },
            
            // October 2 (Italy)
            new RegionalEventDefinition
            {
                EventName = "Grandparents' Day",
                Country = "🇮🇹 Italy",
                CountryCode = "IT",
                DateType = EventDateType.Fixed,
                FixedMonth = 10,
                FixedDay = 2
            },
            
            // ========== EASTER ==========
            
            // Western/Catholic Easter (most countries)
            new RegionalEventDefinition
            {
                EventName = "Easter",
                Country = "🇺🇸 United States",
                CountryCode = "US",
                DateType = EventDateType.Computed,
                ComputedType = "Easter"
            },
            new RegionalEventDefinition
            {
                EventName = "Easter",
                Country = "🇬🇧 United Kingdom",
                CountryCode = "GB",
                DateType = EventDateType.Computed,
                ComputedType = "Easter"
            },
            new RegionalEventDefinition
            {
                EventName = "Easter",
                Country = "🇮🇪 Ireland",
                CountryCode = "IE",
                DateType = EventDateType.Computed,
                ComputedType = "Easter"
            },
            new RegionalEventDefinition
            {
                EventName = "Easter",
                Country = "🇨🇦 Canada",
                CountryCode = "CA",
                DateType = EventDateType.Computed,
                ComputedType = "Easter"
            },
            new RegionalEventDefinition
            {
                EventName = "Easter",
                Country = "🇦🇺 Australia",
                CountryCode = "AU",
                DateType = EventDateType.Computed,
                ComputedType = "Easter"
            },
            new RegionalEventDefinition
            {
                EventName = "Easter",
                Country = "🇳🇿 New Zealand",
                CountryCode = "NZ",
                DateType = EventDateType.Computed,
                ComputedType = "Easter"
            },
            new RegionalEventDefinition
            {
                EventName = "Easter",
                Country = "🇮🇹 Italy",
                CountryCode = "IT",
                DateType = EventDateType.Computed,
                ComputedType = "Easter"
            },
            new RegionalEventDefinition
            {
                EventName = "Easter",
                Country = "🇪🇸 Spain",
                CountryCode = "ES",
                DateType = EventDateType.Computed,
                ComputedType = "Easter"
            },
            new RegionalEventDefinition
            {
                EventName = "Easter",
                Country = "🇫🇷 France",
                CountryCode = "FR",
                DateType = EventDateType.Computed,
                ComputedType = "Easter"
            },
            new RegionalEventDefinition
            {
                EventName = "Easter",
                Country = "🇩🇪 Germany",
                CountryCode = "DE",
                DateType = EventDateType.Computed,
                ComputedType = "Easter"
            },
            new RegionalEventDefinition
            {
                EventName = "Easter",
                Country = "🇵🇹 Portugal",
                CountryCode = "PT",
                DateType = EventDateType.Computed,
                ComputedType = "Easter"
            },
            new RegionalEventDefinition
            {
                EventName = "Easter",
                Country = "🇧🇷 Brazil",
                CountryCode = "BR",
                DateType = EventDateType.Computed,
                ComputedType = "Easter"
            },
            new RegionalEventDefinition
            {
                EventName = "Easter",
                Country = "🇲🇽 Mexico",
                CountryCode = "MX",
                DateType = EventDateType.Computed,
                ComputedType = "Easter"
            },
            new RegionalEventDefinition
            {
                EventName = "Easter",
                Country = "🇵🇱 Poland",
                CountryCode = "PL",
                DateType = EventDateType.Computed,
                ComputedType = "Easter"
            },
            
            // Orthodox Easter (Eastern European countries)
            new RegionalEventDefinition
            {
                EventName = "Easter",
                Country = "🇬🇷 Greece (Orthodox)",
                CountryCode = "GR",
                DateType = EventDateType.Computed,
                ComputedType = "Orthodox Easter"
            },
            new RegionalEventDefinition
            {
                EventName = "Easter",
                Country = "🇷🇺 Russia (Orthodox)",
                CountryCode = "RU",
                DateType = EventDateType.Computed,
                ComputedType = "Orthodox Easter"
            },
            new RegionalEventDefinition
            {
                EventName = "Easter",
                Country = "🇷🇴 Romania (Orthodox)",
                CountryCode = "RO",
                DateType = EventDateType.Computed,
                ComputedType = "Orthodox Easter"
            },
            new RegionalEventDefinition
            {
                EventName = "Easter",
                Country = "🇺🇦 Ukraine (Orthodox)",
                CountryCode = "UA",
                DateType = EventDateType.Computed,
                ComputedType = "Orthodox Easter"
            },
            new RegionalEventDefinition
            {
                EventName = "Easter",
                Country = "🇷🇸 Serbia (Orthodox)",
                CountryCode = "RS",
                DateType = EventDateType.Computed,
                ComputedType = "Orthodox Easter"
            },
            
            // ========== MARDI GRAS (FAT TUESDAY) ==========
            
            // 47 days before Easter (mainly celebrated in Catholic regions)
            new RegionalEventDefinition
            {
                EventName = "Mardi Gras",
                Country = "🇺🇸 United States",
                CountryCode = "US",
                DateType = EventDateType.Computed,
                ComputedType = "Mardi Gras"
            },
            new RegionalEventDefinition
            {
                EventName = "Mardi Gras",
                Country = "🇫🇷 France",
                CountryCode = "FR",
                DateType = EventDateType.Computed,
                ComputedType = "Mardi Gras"
            },
            new RegionalEventDefinition
            {
                EventName = "Mardi Gras",
                Country = "🇧🇷 Brazil",
                CountryCode = "BR",
                DateType = EventDateType.Computed,
                ComputedType = "Mardi Gras"
            },
            new RegionalEventDefinition
            {
                EventName = "Mardi Gras",
                Country = "🇮🇹 Italy",
                CountryCode = "IT",
                DateType = EventDateType.Computed,
                ComputedType = "Mardi Gras"
            },
            new RegionalEventDefinition
            {
                EventName = "Mardi Gras",
                Country = "🇪🇸 Spain",
                CountryCode = "ES",
                DateType = EventDateType.Computed,
                ComputedType = "Mardi Gras"
            },
            new RegionalEventDefinition
            {
                EventName = "Mardi Gras",
                Country = "🇧🇪 Belgium",
                CountryCode = "BE",
                DateType = EventDateType.Computed,
                ComputedType = "Mardi Gras"
            },
            new RegionalEventDefinition
            {
                EventName = "Mardi Gras",
                Country = "🇩🇪 Germany",
                CountryCode = "DE",
                DateType = EventDateType.Computed,
                ComputedType = "Mardi Gras"
            },
            new RegionalEventDefinition
            {
                EventName = "Mardi Gras",
                Country = "🇨🇭 Switzerland",
                CountryCode = "CH",
                DateType = EventDateType.Computed,
                ComputedType = "Mardi Gras"
            }
        };
    }
}
