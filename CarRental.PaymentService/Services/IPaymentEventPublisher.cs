using CarRental.Shared.Events;

namespace CarRental.PaymentService.Services;

// Abstraktion over hvordan Booking Service får besked om en fejlet betaling:
// RabbitMQ (RabbitMqPaymentEventPublisher) eller REST (HttpPaymentEventPublisher).
public interface IPaymentEventPublisher
{
    Task PublishPaymentFailedAsync(PaymentFailedEvent paymentFailed);
}
