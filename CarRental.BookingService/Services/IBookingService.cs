using CarRental.BookingService.Models;

namespace CarRental.BookingService.Services;

public interface IBookingService
{
    // Krav: booking af bil for en valgt periode.
    Task<BookingResult> CreateBookingAsync(Guid customerId, Guid carId, DateTime startDate, DateTime endDate);

    // Krav: annullering af booking.
    Task<BookingResult> CancelBookingAsync(Guid bookingId);

    // Krav: tjek om booking allerede findes.
    Task<bool> HasOverlappingBookingAsync(Guid customerId, Guid carId, DateTime startDate, DateTime endDate);

    // Krav: se booking-historik.
    Task<List<Booking>> GetBookingHistoryAsync(Guid customerId);

    // Kaldes efter Payment Service bekræfter betaling.
    Task<BookingResult> ConfirmBookingAsync(Guid bookingId);

    // Kaldes hvis Payment Service melder fejlet betaling.
    Task HandlePaymentFailedAsync(Guid bookingId);

    // Personale-funktioner.
    Task<BookingResult> ConfirmPickupAsync(Guid bookingId);
    Task<BookingResult> RegisterReturnAsync(Guid bookingId);
}

// Generisk resultat fra alle BookingService-metoder.
public record BookingResult(bool Success, string? Error, Booking? Booking);
