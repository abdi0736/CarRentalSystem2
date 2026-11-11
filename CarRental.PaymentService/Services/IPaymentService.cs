using CarRental.PaymentService.Models;

namespace CarRental.PaymentService.Services;

public interface IPaymentService
{
    // Krav: betaling online, fejlet betaling annullerer bookingen.
    Task<Payment> ProcessPaymentAsync(Guid bookingId, decimal amount);

    Task<Payment?> GetByBookingIdAsync(Guid bookingId);
}
