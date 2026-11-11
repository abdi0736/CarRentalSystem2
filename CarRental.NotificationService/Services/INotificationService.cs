using CarRental.Shared.Events;

namespace CarRental.NotificationService.Services;

public interface INotificationService
{
    // Krav: hvis afsendelse fejler, forbliver bookingen gennemført, og der
    // prøves igen — denne metode kaster derfor aldrig en fejl videre.
    Task NotifyBookingConfirmedAsync(BookingConfirmedEvent bookingConfirmed);
}
