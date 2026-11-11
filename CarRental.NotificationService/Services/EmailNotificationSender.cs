namespace CarRental.NotificationService.Services;

// MIDLERTIDIG: logger bare i stedet for at sende. Skift til rigtig udbyder senere.
public class EmailNotificationSender : INotificationSender
{
    private readonly ILogger<EmailNotificationSender> _logger;

    public EmailNotificationSender(ILogger<EmailNotificationSender> logger)
    {
        _logger = logger;
    }

    public string Channel => "Email";

    public Task<bool> SendAsync(string recipient, string subject, string body)
    {
        _logger.LogInformation("Email to {Recipient}: {Subject}", recipient, subject);
        return Task.FromResult(true);
    }
}
