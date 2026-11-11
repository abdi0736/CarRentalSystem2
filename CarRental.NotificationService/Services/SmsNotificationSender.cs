namespace CarRental.NotificationService.Services;

// MIDLERTIDIG: logger bare i stedet for at sende. Skift til rigtig udbyder senere.
public class SmsNotificationSender : INotificationSender
{
    private readonly ILogger<SmsNotificationSender> _logger;

    public SmsNotificationSender(ILogger<SmsNotificationSender> logger)
    {
        _logger = logger;
    }

    public string Channel => "SMS";

    public Task<bool> SendAsync(string recipient, string subject, string body)
    {
        _logger.LogInformation("SMS to {Recipient}: {Body}", recipient, body);
        return Task.FromResult(true);
    }
}
