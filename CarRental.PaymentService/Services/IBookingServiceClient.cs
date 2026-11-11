namespace CarRental.PaymentService.Services;

public interface IBookingServiceClient
{
    Task ConfirmBookingAsync(Guid bookingId);

    // Krav: fejlet betaling annullerer reservationen.
    Task NotifyPaymentFailedAsync(Guid bookingId);
}
