using System.Text.Json;
using CarRental.PaymentService.Services;
using CarRental.Shared.Events;
using RabbitMQ.Client;

namespace CarRental.PaymentService.Messaging;

// Publicerer PaymentFailedEvent på RabbitMQ. Booking Service lytter på
// routing key "payment.failed" og annullerer reservationen.
public class RabbitMqPaymentEventPublisher : IPaymentEventPublisher
{
    private readonly RabbitMqConnection _connection;
    private readonly ILogger<RabbitMqPaymentEventPublisher> _logger;

    public RabbitMqPaymentEventPublisher(RabbitMqConnection connection, ILogger<RabbitMqPaymentEventPublisher> logger)
    {
        _connection = connection;
        _logger = logger;
    }

    public Task PublishPaymentFailedAsync(PaymentFailedEvent paymentFailed)
    {
        using var channel = _connection.CreateChannel();

        var properties = channel.CreateBasicProperties();
        properties.Persistent = true;
        properties.ContentType = "application/json";
        properties.Type = nameof(PaymentFailedEvent);

        var body = JsonSerializer.SerializeToUtf8Bytes(paymentFailed, RabbitMqConnection.JsonOptions);
        channel.BasicPublish(_connection.Exchange, RoutingKeys.PaymentFailed, properties, body);

        _logger.LogInformation("Published {Event} for booking {BookingId}",
            nameof(PaymentFailedEvent), paymentFailed.BookingId);

        return Task.CompletedTask;
    }
}
