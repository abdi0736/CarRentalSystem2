using System.Text.Json;
using CarRental.BookingService.Services;
using CarRental.Shared.Events;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace CarRental.BookingService.Messaging;

// Lytter på "payment.failed" og annullerer reservationen (saga-kompensation).
// Erstatter PaymentEventsController når RabbitMq:Enabled=true.
public class PaymentFailedConsumer : BackgroundService
{
    private const string QueueName = "booking-service.payment-failed";

    private readonly RabbitMqConnection _connection;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<PaymentFailedConsumer> _logger;
    private IModel? _channel;

    public PaymentFailedConsumer(
        RabbitMqConnection connection,
        IServiceScopeFactory scopeFactory,
        ILogger<PaymentFailedConsumer> logger)
    {
        _connection = connection;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Lad host'en starte færdig før vi (måske) venter på brokeren.
        await Task.Yield();

        var channel = _connection.CreateChannel();
        _channel = channel;

        channel.QueueDeclare(QueueName, durable: true, exclusive: false, autoDelete: false);
        channel.QueueBind(QueueName, _connection.Exchange, RoutingKeys.PaymentFailed);
        channel.BasicQos(prefetchSize: 0, prefetchCount: 10, global: false);

        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.Received += async (_, delivery) =>
        {
            try
            {
                var paymentFailed = Deserialize(delivery.Body);
                if (paymentFailed is not null)
                {
                    // IBookingService er scoped, så der laves et scope pr. besked.
                    using var scope = _scopeFactory.CreateScope();
                    var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();
                    await bookingService.HandlePaymentFailedAsync(paymentFailed.BookingId);

                    _logger.LogInformation("Handled PaymentFailedEvent for booking {BookingId}", paymentFailed.BookingId);
                }

                channel.BasicAck(delivery.DeliveryTag, multiple: false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to handle message on {Queue}", QueueName);
                channel.BasicNack(delivery.DeliveryTag, multiple: false, requeue: false);
            }
        };

        channel.BasicConsume(QueueName, autoAck: false, consumer);
        _logger.LogInformation("Listening on queue {Queue}", QueueName);
    }

    private static PaymentFailedEvent? Deserialize(ReadOnlyMemory<byte> body) =>
        JsonSerializer.Deserialize<PaymentFailedEvent>(body.Span, RabbitMqConnection.JsonOptions);

    public override void Dispose()
    {
        _channel?.Dispose();
        base.Dispose();
    }
}
