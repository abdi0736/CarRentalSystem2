using System.Text.Json;
using CarRental.NotificationService.Messaging;
using CarRental.NotificationService.Services;
using CarRental.Shared.Events;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace CarRental.NotificationService.Consumers;

// Lytter på "booking.confirmed"-køen og sender email/SMS for hver besked.
// Erstatter NotificationController når RabbitMq:Enabled=true.
public class BookingConfirmedConsumer : BackgroundService
{
    private const string QueueName = "notification-service.booking-confirmed";

    private readonly RabbitMqConnection _connection;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<BookingConfirmedConsumer> _logger;
    private IModel? _channel;

    public BookingConfirmedConsumer(
        RabbitMqConnection connection,
        IServiceScopeFactory scopeFactory,
        ILogger<BookingConfirmedConsumer> logger)
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
        channel.QueueBind(QueueName, _connection.Exchange, RoutingKeys.BookingConfirmed);
        channel.BasicQos(prefetchSize: 0, prefetchCount: 10, global: false);

        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.Received += async (_, delivery) =>
        {
            try
            {
                var bookingConfirmed = Deserialize(delivery.Body);
                if (bookingConfirmed is not null)
                {
                    using var scope = _scopeFactory.CreateScope();
                    var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

                    // Kaster aldrig videre — retry håndteres inde i NotificationService.
                    await notificationService.NotifyBookingConfirmedAsync(bookingConfirmed);
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

    private static BookingConfirmedEvent? Deserialize(ReadOnlyMemory<byte> body) =>
        JsonSerializer.Deserialize<BookingConfirmedEvent>(body.Span, RabbitMqConnection.JsonOptions);

    public override void Dispose()
    {
        _channel?.Dispose();
        base.Dispose();
    }
}
