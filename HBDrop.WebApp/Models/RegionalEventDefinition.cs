using System.ComponentModel.DataAnnotations;

namespace HBDrop.WebApp.Models;

/// <summary>
/// Represents a well-known event that has different dates in different countries/regions
/// </summary>
public class RegionalEventDefinition
{
    public string EventName { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string CountryCode { get; set; } = string.Empty;
    public EventDateType DateType { get; set; }
    
    // For fixed dates
    public int? FixedMonth { get; set; }
    public int? FixedDay { get; set; }
    
    // For floating dates (e.g., "3rd Sunday of June")
    public int? FloatingMonth { get; set; }
    public DayOfWeek? FloatingDayOfWeek { get; set; }
    public int? FloatingWeekNumber { get; set; } // 1-5, or -1 for last
    
    // For computed dates (e.g., Easter)
    public string? ComputedType { get; set; } // "Easter", "Orthodox Easter", "Mardi Gras"
    
    /// <summary>
    /// Calculate the date for this event in a given year
    /// </summary>
    public DateTime CalculateDateForYear(int year)
    {
        if (DateType == EventDateType.Fixed && FixedMonth.HasValue && FixedDay.HasValue)
        {
            return new DateTime(year, FixedMonth.Value, FixedDay.Value);
        }
        else if (DateType == EventDateType.Floating && FloatingMonth.HasValue && 
                 FloatingDayOfWeek.HasValue && FloatingWeekNumber.HasValue)
        {
            return CalculateFloatingDate(year, FloatingMonth.Value, FloatingDayOfWeek.Value, FloatingWeekNumber.Value);
        }
        else if (DateType == EventDateType.Computed && !string.IsNullOrEmpty(ComputedType))
        {
            return CalculateComputedDate(year, ComputedType);
        }
        
        throw new InvalidOperationException("Invalid event definition");
    }
    
    private DateTime CalculateFloatingDate(int year, int month, DayOfWeek dayOfWeek, int weekNumber)
    {
        if (weekNumber == -1)
        {
            // Last occurrence of the day in the month
            var lastDayOfMonth = new DateTime(year, month, DateTime.DaysInMonth(year, month));
            while (lastDayOfMonth.DayOfWeek != dayOfWeek)
            {
                lastDayOfMonth = lastDayOfMonth.AddDays(-1);
            }
            return lastDayOfMonth;
        }
        else
        {
            // Nth occurrence of the day in the month
            var firstDayOfMonth = new DateTime(year, month, 1);
            int daysUntilTarget = ((int)dayOfWeek - (int)firstDayOfMonth.DayOfWeek + 7) % 7;
            var firstOccurrence = firstDayOfMonth.AddDays(daysUntilTarget);
            return firstOccurrence.AddDays((weekNumber - 1) * 7);
        }
    }
    
    private DateTime CalculateComputedDate(int year, string computedType)
    {
        return computedType switch
        {
            "Easter" => CalculateEaster(year),
            "Orthodox Easter" => CalculateOrthodoxEaster(year),
            "Mardi Gras" => CalculateEaster(year).AddDays(-47), // 47 days before Easter
            _ => throw new InvalidOperationException($"Unknown computed type: {computedType}")
        };
    }
    
    /// <summary>
    /// Calculate Western/Catholic Easter using the Anonymous Gregorian algorithm
    /// </summary>
    private DateTime CalculateEaster(int year)
    {
        int a = year % 19;
        int b = year / 100;
        int c = year % 100;
        int d = b / 4;
        int e = b % 4;
        int f = (b + 8) / 25;
        int g = (b - f + 1) / 3;
        int h = (19 * a + b - d - g + 15) % 30;
        int i = c / 4;
        int k = c % 4;
        int l = (32 + 2 * e + 2 * i - h - k) % 7;
        int m = (a + 11 * h + 22 * l) / 451;
        int month = (h + l - 7 * m + 114) / 31;
        int day = ((h + l - 7 * m + 114) % 31) + 1;
        
        return new DateTime(year, month, day);
    }
    
    /// <summary>
    /// Calculate Eastern Orthodox Easter using the Julian calendar calculation
    /// </summary>
    private DateTime CalculateOrthodoxEaster(int year)
    {
        int a = year % 4;
        int b = year % 7;
        int c = year % 19;
        int d = (19 * c + 15) % 30;
        int e = (2 * a + 4 * b - d + 34) % 7;
        int month = (d + e + 114) / 31;
        int day = ((d + e + 114) % 31) + 1;
        
        var julianEaster = new DateTime(year, month, day);
        
        // Convert from Julian to Gregorian calendar (add 13 days for 20th-21st century)
        return julianEaster.AddDays(13);
    }
}

public enum EventDateType
{
    Fixed,      // Same date every year (e.g., March 19)
    Floating,   // Relative date (e.g., 3rd Sunday of June)
    Computed    // Algorithmically calculated (e.g., Easter)
}
