using CarRental.Shared.Events;

namespace CarRental.PaymentService.Services;

// Fallback når RabbitMq:Enabled=false: kalder Booking Service direkte over REST
// (den oprindelige MIDLERTIDIGE løsning via PaymentEventsController).
public class HttpPaymentEventPublisher : IPaymentEventPublisher
{
    private readonly IBookingServiceClient _bookingServiceClient;

    public HttpPaymentEventPublisher(IBookingServiceClient bookingServiceClient)
    {
        _bookingServiceClient = bookingServiceClient;
    }

    public Task PublishPaymentFailedAsync(PaymentFailedEvent paymentFailed) =>
        _bookingServiceClient.NotifyPaymentFailedAsync(paymentFailed.BookingId);
}
