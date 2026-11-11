namespace CarRental.NotificationService.Services;

// Krav: bekræftelse sendes via email/SMS. Én kanal ad gangen, så en fejlende
// SMS-udbyder ikke blokerer email, og hver kan skiftes uafhængigt.
public interface INotificationSender
{
    string Channel { get; }
    Task<bool> SendAsync(string recipient, string subject, string body);
}
