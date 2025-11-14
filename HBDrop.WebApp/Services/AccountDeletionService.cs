using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using HBDrop.WebApp.Data;
using HBDrop.WebApp.Models;

namespace HBDrop.WebApp.Services;

/// <summary>
/// Service to handle complete account deletion and cleanup of all related data
/// </summary>
public class AccountDeletionService
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<AccountDeletionService> _logger;

    public AccountDeletionService(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        ILogger<AccountDeletionService> logger)
    {
        _context = context;
        _userManager = userManager;
        _logger = logger;
    }

    /// <summary>
    /// Deletes a user account and all associated data
    /// </summary>
    /// <param name="userId">The ID of the user to delete</param>
    /// <returns>Result containing success status and any error messages</returns>
    public async Task<AccountDeletionResult> DeleteAccountAsync(string userId)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        
        try
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return new AccountDeletionResult 
                { 
                    Success = false, 
                    ErrorMessage = "User not found" 
                };
            }

            _logger.LogInformation("Starting account deletion for user {UserId} ({Email})", userId, user.Email);

            // 1. Delete WhatsApp Session
            var whatsAppSession = await _context.WhatsAppSessions
                .FirstOrDefaultAsync(w => w.UserId == userId);
            if (whatsAppSession != null)
            {
                _context.WhatsAppSessions.Remove(whatsAppSession);
                _logger.LogInformation("Deleted WhatsApp session for user {UserId}", userId);
            }

            // 2. Delete Messages (both sent and received through contacts)
            var messages = await _context.Messages
                .Where(m => m.UserId == userId)
                .ToListAsync();
            if (messages.Any())
            {
                _context.Messages.RemoveRange(messages);
                _logger.LogInformation("Deleted {Count} messages for user {UserId}", messages.Count, userId);
            }

            // 3. Get all user's contacts (we need them for cascading deletions)
            var contacts = await _context.Contacts
                .Include(c => c.Birthdays)
                .Include(c => c.AdditionalBirthdays)
                .Include(c => c.CustomEvents)
                .Where(c => c.UserId == userId)
                .ToListAsync();

            if (contacts.Any())
            {
                // Delete Custom Events
                var customEvents = contacts.SelectMany(c => c.CustomEvents).ToList();
                if (customEvents.Any())
                {
                    _context.CustomEvents.RemoveRange(customEvents);
                    _logger.LogInformation("Deleted {Count} custom events for user {UserId}", customEvents.Count, userId);
                }

                // Delete Additional Birthdays
                var additionalBirthdays = contacts.SelectMany(c => c.AdditionalBirthdays).ToList();
                if (additionalBirthdays.Any())
                {
                    _context.AdditionalBirthdays.RemoveRange(additionalBirthdays);
                    _logger.LogInformation("Deleted {Count} additional birthdays for user {UserId}", additionalBirthdays.Count, userId);
                }

                // Delete Birthdays
                var birthdays = contacts.SelectMany(c => c.Birthdays).ToList();
                if (birthdays.Any())
                {
                    _context.Birthdays.RemoveRange(birthdays);
                    _logger.LogInformation("Deleted {Count} birthdays for user {UserId}", birthdays.Count, userId);
                }

                // Delete Contacts
                _context.Contacts.RemoveRange(contacts);
                _logger.LogInformation("Deleted {Count} contacts for user {UserId}", contacts.Count, userId);
            }

            // 4. Save all entity deletions before deleting the user
            await _context.SaveChangesAsync();

            // 5. Delete the user account (this will also handle Identity tables)
            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                _logger.LogError("Failed to delete user {UserId}: {Errors}", 
                    userId, 
                    string.Join(", ", result.Errors.Select(e => e.Description)));
                
                await transaction.RollbackAsync();
                return new AccountDeletionResult
                {
                    Success = false,
                    ErrorMessage = "Failed to delete user account: " + 
                        string.Join(", ", result.Errors.Select(e => e.Description))
                };
            }

            // 6. Commit the transaction
            // Note: SignOut is handled by the calling page to avoid "Headers are read-only" errors
            await transaction.CommitAsync();

            _logger.LogInformation("Successfully deleted account for user {UserId} ({Email})", userId, user.Email);

            return new AccountDeletionResult
            {
                Success = true,
                ErrorMessage = null
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting account for user {UserId}", userId);
            await transaction.RollbackAsync();
            
            return new AccountDeletionResult
            {
                Success = false,
                ErrorMessage = $"An error occurred while deleting the account: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// Gets a summary of all data that will be deleted for a user
    /// </summary>
    public async Task<AccountDataSummary> GetAccountDataSummaryAsync(string userId)
    {
        try
        {
            var contactsCount = await _context.Contacts.CountAsync(c => c.UserId == userId);
            var birthdaysCount = await _context.Birthdays
                .Include(b => b.Contact)
                .CountAsync(b => b.Contact.UserId == userId);
            var additionalBirthdaysCount = await _context.AdditionalBirthdays
                .Include(ab => ab.Contact)
                .CountAsync(ab => ab.Contact.UserId == userId);
            var customEventsCount = await _context.CustomEvents
                .Include(ce => ce.Contact)
                .CountAsync(ce => ce.Contact.UserId == userId);
            var messagesCount = await _context.Messages.CountAsync(m => m.UserId == userId);
            var hasWhatsAppSession = await _context.WhatsAppSessions.AnyAsync(w => w.UserId == userId);

            return new AccountDataSummary
            {
                ContactsCount = contactsCount,
                BirthdaysCount = birthdaysCount,
                AdditionalBirthdaysCount = additionalBirthdaysCount,
                CustomEventsCount = customEventsCount,
                MessagesCount = messagesCount,
                HasWhatsAppSession = hasWhatsAppSession
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting account data summary for user {UserId}", userId);
            return new AccountDataSummary();
        }
    }
}

/// <summary>
/// Result of an account deletion operation
/// </summary>
public class AccountDeletionResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Summary of data associated with an account
/// </summary>
public class AccountDataSummary
{
    public int ContactsCount { get; set; }
    public int BirthdaysCount { get; set; }
    public int AdditionalBirthdaysCount { get; set; }
    public int CustomEventsCount { get; set; }
    public int MessagesCount { get; set; }
    public bool HasWhatsAppSession { get; set; }
}
