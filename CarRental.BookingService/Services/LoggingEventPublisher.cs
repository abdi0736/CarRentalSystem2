using CarRental.Shared.Events;

namespace CarRental.BookingService.Services;

// MIDLERTIDIG stand-in til rigtig message broker er sat op.
// NB: bruges ikke — RabbitMqEventPublisher eller HttpEventPublisher registreres i stedet.
public class LoggingEventPublisher : IEventPublisher
{
    private readonly ILogger<LoggingEventPublisher> _logger;

    public LoggingEventPublisher(ILogger<LoggingEventPublisher> logger)
    {
        _logger = logger;
    }

    public Task PublishBookingConfirmedAsync(BookingConfirmedEvent bookingConfirmed)
    {
        _logger.LogInformation(
            "BookingConfirmedEvent published for booking {BookingId}",
            bookingConfirmed.BookingId);

        return Task.CompletedTask;
    }
}
