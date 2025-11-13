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
            var aiService = scope.ServiceProvider.GetRequiredService<AIMessageService>();

            // Check main birthdays
            await CheckMainBirthdaysAsync(dbContext, whatsAppService, aiService);
            
            // Check additional birthdays
            await CheckAdditionalBirthdaysAsync(dbContext, whatsAppService, aiService);
            
            // Check custom events
            await CheckCustomEventsAsync(dbContext, whatsAppService, aiService);

            _logger.LogInformation("Birthday check job completed at {Time}", DateTime.UtcNow);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fatal error in birthday check job");
        }
    }

    private async Task CheckMainBirthdaysAsync(
        ApplicationDbContext dbContext,
        IWhatsAppService whatsAppService,
        AIMessageService aiService)
    {

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
                            var message = await GenerateBirthdayMessageAsync(birthday, contact, aiService);

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
                                message,
                                birthday.GifUrl
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
        }

        _logger.LogInformation("Completed checking main birthdays");
    }

    private async Task CheckAdditionalBirthdaysAsync(
        ApplicationDbContext dbContext,
        IWhatsAppService whatsAppService,
        AIMessageService aiService)
    {
        // Get all enabled additional birthdays with their contacts
        var additionalBirthdays = await dbContext.AdditionalBirthdays
            .Include(ab => ab.Contact)
            .ThenInclude(c => c.User)
            .Where(ab => ab.IsEnabled)
            .ToListAsync();

        _logger.LogInformation("Found {Count} enabled additional birthdays to check", additionalBirthdays.Count);

        foreach (var additionalBirthday in additionalBirthdays)
        {
            try
            {
                var contact = additionalBirthday.Contact;
                var userId = contact.UserId;

                // Check if it's time to send message
                if (additionalBirthday.IsTimeToSendMessage(additionalBirthday.TimeZoneId, additionalBirthday.MessageHour))
                {
                    _logger.LogInformation(
                        "It's {Name}'s birthday (Additional)! Sending message to {DestinationType} (User: {UserId})",
                        additionalBirthday.Name,
                        additionalBirthday.SendTo,
                        userId
                    );

                    // Generate message
                    var message = await GenerateAdditionalBirthdayMessageAsync(additionalBirthday, contact, aiService);

                    // Determine where to send
                    var destinationNumber = additionalBirthday.SendTo == "Contact" 
                        ? contact.PhoneNumber 
                        : additionalBirthday.SendToGroupId;

                    if (string.IsNullOrEmpty(destinationNumber))
                    {
                        _logger.LogWarning("No destination number for additional birthday {Name}", additionalBirthday.Name);
                        continue;
                    }

                    // Send via WhatsApp
                    var success = await SendWhatsAppMessage(
                        whatsAppService,
                        userId,
                        destinationNumber,
                        message,
                        additionalBirthday.GifUrl
                    );

                    if (success)
                    {
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
                            "Successfully sent additional birthday message for {Name} (Contact: {ContactName})",
                            additionalBirthday.Name,
                            contact.Name
                        );
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error processing additional birthday {Id} for contact {ContactId}",
                    additionalBirthday.Id,
                    additionalBirthday.ContactId
                );
            }
        }

        _logger.LogInformation("Completed checking additional birthdays");
    }

    private async Task CheckCustomEventsAsync(
        ApplicationDbContext dbContext,
        IWhatsAppService whatsAppService,
        AIMessageService aiService)
    {
        // Get all enabled custom events with their contacts
        var customEvents = await dbContext.CustomEvents
            .Include(ce => ce.Contact)
            .ThenInclude(c => c.User)
            .Where(ce => ce.IsEnabled)
            .ToListAsync();

        _logger.LogInformation("Found {Count} enabled custom events to check", customEvents.Count);

        foreach (var customEvent in customEvents)
        {
            try
            {
                var contact = customEvent.Contact;
                var userId = contact.UserId;

                // Check if it's time to send message
                if (customEvent.IsTimeToSendMessage(customEvent.TimeZoneId, customEvent.MessageHour))
                {
                    _logger.LogInformation(
                        "It's time for custom event '{EventName}' for {ContactName} (User: {UserId})",
                        customEvent.EventName,
                        contact.Name,
                        userId
                    );

                    // Generate message
                    var message = await GenerateCustomEventMessageAsync(customEvent, contact, aiService);

                    // Determine where to send
                    var destinationNumber = customEvent.GroupId.HasValue && customEvent.GroupId > 0
                        ? (await dbContext.Contacts.FindAsync(customEvent.GroupId.Value))?.PhoneNumber
                        : contact.PhoneNumber;

                    if (string.IsNullOrEmpty(destinationNumber))
                    {
                        _logger.LogWarning("No destination number for custom event {EventName}", customEvent.EventName);
                        continue;
                    }

                    // Send via WhatsApp
                    var success = await SendWhatsAppMessage(
                        whatsAppService,
                        userId,
                        destinationNumber,
                        message,
                        customEvent.GifUrl
                    );

                    if (success)
                    {
                        // Log in database
                        var messageLog = new Message
                        {
                            UserId = userId,
                            ContactId = contact.Id,
                            Content = message,
                            SentAt = DateTime.UtcNow,
                            Status = MessageStatus.Sent,
                            IsBirthdayMessage = false // Custom event, not a birthday
                        };

                        dbContext.Messages.Add(messageLog);
                        await dbContext.SaveChangesAsync();

                        _logger.LogInformation(
                            "Successfully sent custom event message '{EventName}' to {ContactName}",
                            customEvent.EventName,
                            contact.Name
                        );
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error processing custom event {Id} for contact {ContactId}",
                    customEvent.Id,
                    customEvent.ContactId
                );
            }
        }

        _logger.LogInformation("Completed checking custom events");
    }

            _logger.LogInformation("Birthday check job completed at {Time}", DateTime.UtcNow);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fatal error in birthday check job");
        }
    }

    private async Task<string> GenerateBirthdayMessageAsync(Birthday birthday, Contact contact, AIMessageService aiService)
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

        // Try to generate AI message if no custom message is set
        try
        {
            _logger.LogInformation("Generating AI birthday message for {Name}", contact.Name);
            
            var aiMessage = await aiService.GenerateMessageAsync(
                displayName,
                "birthday",
                contact.Notes
            );
            
            if (!string.IsNullOrWhiteSpace(aiMessage))
            {
                _logger.LogInformation("Successfully generated AI message for {Name}", contact.Name);
                return aiMessage;
            }
            
            _logger.LogWarning("AI service returned empty message for {Name}, using default", contact.Name);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to generate AI message for {Name}, using default", contact.Name);
        }

        // Fallback to default message template if AI fails
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
        string message,
        string? gifUrl = null)
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
            var success = await whatsAppService.SendMessageAsync(userId, phoneNumber, message, gifUrl);
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
