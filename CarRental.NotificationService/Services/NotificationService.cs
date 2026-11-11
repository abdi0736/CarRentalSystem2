using CarRental.Shared.Events;

namespace CarRental.NotificationService.Services;

// Krav: ingen egen database — retry-status holdes kun i memory.
public class NotificationService : INotificationService
{
    private const int MaxAttempts = 3;
    private static readonly TimeSpan DelayBetweenAttempts = TimeSpan.FromSeconds(2);

    // IEnumerable fordi Program.cs registrerer to sendere (Email + SMS).
    private readonly IEnumerable<INotificationSender> _senders;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(IEnumerable<INotificationSender> senders, ILogger<NotificationService> logger)
    {
        _senders = senders;
        _logger = logger;
    }

    // Krav: bekræftelse sendes via email/SMS.
    public async Task NotifyBookingConfirmedAsync(BookingConfirmedEvent bookingConfirmed)
    {
        var subject = "Your car rental booking is confirmed";
        var body = $"Booking {bookingConfirmed.BookingId} is confirmed for " +
                   $"{bookingConfirmed.StartDate:d} to {bookingConfirmed.EndDate:d}. " +
                   $"Total price: {bookingConfirmed.TotalPrice:C}.";

        foreach (var sender in _senders)
        {
            await SendWithRetryAsync(sender, bookingConfirmed.CustomerEmail, subject, body);
        }
    }

    // Krav: fejlet afsendelse prøves igen, uden at påvirke bookingen.
    private async Task SendWithRetryAsync(INotificationSender sender, string recipient, string subject, string body)
    {
        for (var attempt = 1; attempt <= MaxAttempts; attempt++)
        {
            var success = await sender.SendAsync(recipient, subject, body);

            if (success)
                return;

            _logger.LogWarning(
                "{Channel} notification attempt {Attempt}/{Max} failed for {Recipient}",
                sender.Channel, attempt, MaxAttempts, recipient);

            if (attempt < MaxAttempts)
                await Task.Delay(DelayBetweenAttempts);
        }

        _logger.LogError(
            "{Channel} notification permanently failed for {Recipient} after {Max} attempts",
            sender.Channel, recipient, MaxAttempts);
    }
}
