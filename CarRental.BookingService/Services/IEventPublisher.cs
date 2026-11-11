using CarRental.Shared.Events;

namespace CarRental.BookingService.Services;

// Abstraktion over message broker (RabbitMQ/Kafka). Sendes asynkront, så en
// fejlende Notification Service aldrig kan blokere en bekræftet booking.
public interface IEventPublisher
{
    Task PublishBookingConfirmedAsync(BookingConfirmedEvent bookingConfirmed);
}
