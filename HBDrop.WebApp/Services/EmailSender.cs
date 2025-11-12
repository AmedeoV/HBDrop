using Microsoft.AspNetCore.Identity.UI.Services;

namespace HBDrop.WebApp.Services;

public class EmailSender : IEmailSender
{
    public Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        // TODO: Implement email sending with a service like SendGrid, Mailgun, etc.
        // For now, just log it
        Console.WriteLine($"Email to {email}: {subject}");
        return Task.CompletedTask;
    }
}
