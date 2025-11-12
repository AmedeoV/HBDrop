using HBDrop.WebApp.Data;
using HBDrop.WebApp.Models;
using HBDrop.WebApp.Services;
using Microsoft.EntityFrameworkCore;

namespace HBDrop.WebApp.Jobs;

/// <summary>
/// Background job that checks for birthdays and sends automated WhatsApp messages
/// considering each contact's timezone and preferred message time
/// </summary>
public class BirthdayCheckerJob
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<BirthdayCheckerJob> _logger;

    public BirthdayCheckerJob(
        IServiceScopeFactory serviceScopeFactory,
        ILogger<BirthdayCheckerJob> logger)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
    }

    /// <summary>
    /// Check for birthdays and send messages. Should be run every hour.
    /// </summary>
    public async Task CheckAndSendBirthdayWishesAsync()
    {
        _logger.LogInformation("Starting birthday check job at {Time}", DateTime.UtcNow);

        try
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var whatsAppService = scope.ServiceProvider.GetRequiredService<IWhatsAppService>();

            // Get all enabled birthdays with their contacts
            var birthdays = await dbContext.Birthdays
                .Include(b => b.Contact)
                .ThenInclude(c => c.User)
                .Where(b => b.IsEnabled)
                .ToListAsync();

            _logger.LogInformation("Found {Count} enabled birthdays to check", birthdays.Count);

            // Group by user for better logging
            var birthdaysByUser = birthdays.GroupBy(b => b.Contact.UserId);
            _logger.LogInformation("Processing birthdays for {UserCount} users", birthdaysByUser.Count());

            foreach (var userGroup in birthdaysByUser)
            {
                var userId = userGroup.Key;
                var userBirthdays = userGroup.ToList();
                
                _logger.LogInformation(
                    "Processing {Count} birthdays for user {UserId}",
                    userBirthdays.Count,
                    userId
                );

                foreach (var birthday in userBirthdays)
                {
                    try
                    {
                        var contact = birthday.Contact;
                        
                        // Check if it's time to send message based on contact's timezone
                        if (birthday.IsTimeToSendMessage(contact.TimeZoneId, contact.PreferredMessageHour))
                        {
                            _logger.LogInformation(
                                "It's {Name}'s birthday! Sending message to {Phone} (User: {UserId}, Timezone: {TZ}, Hour: {Hour})",
                                contact.Name,
                                contact.PhoneNumber,
                                userId,
                                contact.TimeZoneId ?? "UTC",
                                contact.PreferredMessageHour
                            );

                            // Generate birthday message
                            var message = GenerateBirthdayMessage(birthday, contact);

                            // Determine where to send the message (group or directly to contact)
                            var destinationNumber = string.IsNullOrWhiteSpace(birthday.SendToGroupId) 
                                ? contact.PhoneNumber 
                                : birthday.SendToGroupId;
                            
                            _logger.LogInformation(
                                "Sending birthday message for {Name} to {Destination} (Type: {Type})",
                                contact.Name,
                                destinationNumber,
                                string.IsNullOrWhiteSpace(birthday.SendToGroupId) ? "Direct" : "Group"
                            );

                            // Send via WhatsApp
                            var success = await SendWhatsAppMessage(
                                whatsAppService,
                                userId,
                                destinationNumber,
                                message
                            );

                            if (success)
                            {
                                // Update last sent timestamp
                                birthday.LastSentAt = DateTime.UtcNow;
                                
                                // Log in database
                                var messageLog = new Message
                                {
                                    UserId = userId,
                                    ContactId = contact.Id,
                                    Content = message,
                                    SentAt = DateTime.UtcNow,
                                    Status = MessageStatus.Sent,
                                    IsBirthdayMessage = true
                                };
                                
                                dbContext.Messages.Add(messageLog);
                                await dbContext.SaveChangesAsync();

                                _logger.LogInformation(
                                    "Successfully sent birthday message to {Name} ({Phone}) from user {UserId}",
                                    contact.Name,
                                    contact.PhoneNumber,
                                    userId
                                );
                            }
                            else
                            {
                                _logger.LogWarning(
                                    "Failed to send birthday message to {Name} ({Phone}) from user {UserId}",
                                    contact.Name,
                                    contact.PhoneNumber,
                                    userId
                                );
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(
                            ex,
                            "Error processing birthday for contact {ContactId} (User: {UserId})",
                            birthday.ContactId,
                            userId
                        );
                    }
                }
            }

            _logger.LogInformation("Birthday check job completed at {Time}", DateTime.UtcNow);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fatal error in birthday check job");
        }
    }

    private string GenerateBirthdayMessage(Birthday birthday, Contact contact)
    {
        // Use DisplayName if set, otherwise fall back to first name
        var displayName = !string.IsNullOrWhiteSpace(contact.DisplayName) 
            ? contact.DisplayName 
            : contact.Name.Split(' ')[0];
        
        // Use custom message if available
        if (!string.IsNullOrWhiteSpace(birthday.CustomMessage))
        {
            var customMessage = birthday.CustomMessage;
            var contactAge = birthday.CalculateAge();
            
            // Replace placeholders
            customMessage = customMessage.Replace("{Name}", displayName);
            if (contactAge.HasValue)
            {
                customMessage = customMessage.Replace("{Age}", contactAge.Value.ToString());
            }
            
            return customMessage;
        }

        // Default message template
        var age = birthday.CalculateAge();

        if (age.HasValue)
        {
            return $"🎉 Happy Birthday, {displayName}! 🎂\n\n" +
                   $"Wishing you an amazing {age}th birthday filled with joy, laughter, and wonderful memories! 🎈✨\n\n" +
                   $"Hope your special day is as incredible as you are! 🎁🥳";
        }
        else
        {
            return $"🎉 Happy Birthday, {displayName}! 🎂\n\n" +
                   $"Wishing you a day filled with joy, laughter, and wonderful memories! 🎈✨\n\n" +
                   $"Hope your special day is as incredible as you are! 🎁🥳";
        }
    }

    private async Task<bool> SendWhatsAppMessage(
        IWhatsAppService whatsAppService,
        string userId,
        string phoneNumber,
        string message)
    {
        try
        {
            // Check if user is connected to WhatsApp
            var connectionStatus = await whatsAppService.GetConnectionStatusAsync(userId);
            if (connectionStatus == null || !connectionStatus.IsConnected)
            {
                _logger.LogWarning(
                    "User {UserId} is not connected to WhatsApp. Skipping message.",
                    userId
                );
                return false;
            }

            // Send message using the overload that accepts userId directly (for background jobs)
            var success = await whatsAppService.SendMessageAsync(userId, phoneNumber, message);
            return success;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error sending WhatsApp message to {Phone} for user {UserId}",
                phoneNumber,
                userId
            );
            return false;
        }
    }
}
