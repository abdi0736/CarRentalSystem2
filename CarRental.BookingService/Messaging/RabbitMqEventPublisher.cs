using System.Text.Json;
using CarRental.BookingService.Services;
using CarRental.Shared.Events;
using RabbitMQ.Client;

namespace CarRental.BookingService.Messaging;

// Publicerer BookingConfirmedEvent på RabbitMQ (erstatter HttpEventPublisher
// når RabbitMq:Enabled=true). Notification Service lytter på routing key
// "booking.confirmed" — Booking Service ved ikke hvem der modtager eventet.
public class RabbitMqEventPublisher : IEventPublisher
{
    private readonly RabbitMqConnection _connection;
    private readonly ILogger<RabbitMqEventPublisher> _logger;

    public RabbitMqEventPublisher(RabbitMqConnection connection, ILogger<RabbitMqEventPublisher> logger)
    {
        _connection = connection;
        _logger = logger;
    }

    public Task PublishBookingConfirmedAsync(BookingConfirmedEvent bookingConfirmed)
    {
        try
        {
            using var channel = _connection.CreateChannel();

            var properties = channel.CreateBasicProperties();
            properties.Persistent = true;
            properties.ContentType = "application/json";
            properties.Type = nameof(BookingConfirmedEvent);

            var body = JsonSerializer.SerializeToUtf8Bytes(bookingConfirmed, RabbitMqConnection.JsonOptions);
            channel.BasicPublish(_connection.Exchange, RoutingKeys.BookingConfirmed, properties, body);

            _logger.LogInformation("Published {Event} for booking {BookingId}",
                nameof(BookingConfirmedEvent), bookingConfirmed.BookingId);
        }
        catch (Exception ex)
        {
            // En fejlende broker må aldrig påvirke en bekræftet booking.
            _logger.LogWarning(ex, "Could not publish BookingConfirmedEvent for booking {BookingId}",
                bookingConfirmed.BookingId);
        }

        return Task.CompletedTask;
    }
}
