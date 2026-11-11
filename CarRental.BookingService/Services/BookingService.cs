using System.Collections.Concurrent;
using CarRental.BookingService.Models;
using CarRental.Shared.Enums;
using CarRental.Shared.Events;

namespace CarRental.BookingService.Services;

// NB: gemmer i memory for nu — skiftes til EF Core-database senere.
public class BookingService : IBookingService
{
    private static readonly ConcurrentDictionary<Guid, Booking> Bookings = new();

    private readonly ICarServiceClient _carServiceClient;
    private readonly IUserServiceClient _userServiceClient;
    private readonly IEventPublisher _eventPublisher;

    public BookingService(
        ICarServiceClient carServiceClient,
        IUserServiceClient userServiceClient,
        IEventPublisher eventPublisher)
    {
        _carServiceClient = carServiceClient;
        _userServiceClient = userServiceClient;
        _eventPublisher = eventPublisher;
    }

    // Krav: booking af bil — tjekker duplikat, eligibility, tilgængelighed,
    // og reserverer bilen, i den rækkefølge, før noget gemmes.
    public async Task<BookingResult> CreateBookingAsync(Guid customerId, Guid carId, DateTime startDate, DateTime endDate)
    {
        if (endDate <= startDate)
            return new BookingResult(false, "End date must be after start date.", null);

        // Krav: tjek om booking allerede findes.
        if (await HasOverlappingBookingAsync(customerId, carId, startDate, endDate))
            return new BookingResult(false, "An overlapping booking already exists for this car.", null);

        // Krav: 21+ og gyldigt kørekort.
        var eligibility = await _userServiceClient.CheckEligibilityAsync(customerId);
        if (eligibility is null || !eligibility.IsEligible)
            return new BookingResult(false, eligibility?.Reason ?? "Customer is not eligible to rent.", null);

        // Krav: realtidstjek af tilgængelighed.
        var availability = await _carServiceClient.CheckAvailabilityAsync(carId, startDate, endDate);
        if (availability is null || !availability.IsAvailable)
            return new BookingResult(false, "Car is not available for the selected period.", null);

        // Krav: først-til-mølle, aldrig double booking — atomisk hos Car Service.
        var reserved = await _carServiceClient.TryReserveAsync(carId);
        if (!reserved)
            return new BookingResult(false, "Car was just reserved by another customer.", null);

        var booking = new Booking
        {
            CustomerId = customerId,
            CarId = carId,
            StartDate = startDate,
            EndDate = endDate,
            PricePerDay = availability.PricePerDay,
            Status = BookingStatus.Pending
        };

        Bookings[booking.Id] = booking;
        return new BookingResult(true, null, booking);
    }

    // Krav: tjek om booking allerede findes (overlap på samme bil).
    public Task<bool> HasOverlappingBookingAsync(Guid customerId, Guid carId, DateTime startDate, DateTime endDate)
    {
        var overlaps = Bookings.Values.Any(b =>
            b.CarId == carId &&
            b.Status is BookingStatus.Pending or BookingStatus.Confirmed or BookingStatus.PickedUp &&
            startDate < b.EndDate && endDate > b.StartDate);

        return Task.FromResult(overlaps);
    }

    // Krav: booking-historik.
    public Task<List<Booking>> GetBookingHistoryAsync(Guid customerId)
    {
        var history = Bookings.Values
            .Where(b => b.CustomerId == customerId)
            .OrderByDescending(b => b.CreatedAt)
            .ToList();

        return Task.FromResult(history);
    }

    // Kaldes efter vellykket betaling.
    public async Task<BookingResult> ConfirmBookingAsync(Guid bookingId)
    {
        if (!Bookings.TryGetValue(bookingId, out var booking))
            return new BookingResult(false, "Booking not found.", null);

        booking.Status = BookingStatus.Confirmed;

        // Krav: bekræftelse sendes asynkront, Notification Service-fejl må ikke ramme bookingen.
        await _eventPublisher.PublishBookingConfirmedAsync(new BookingConfirmedEvent
        {
            BookingId = booking.Id,
            CustomerId = booking.CustomerId,
            CarId = booking.CarId,
            StartDate = booking.StartDate,
            EndDate = booking.EndDate,
            TotalPrice = booking.TotalPrice
        });

        return new BookingResult(true, null, booking);
    }

    // Krav: fejlet betaling annullerer reservationen.
    public async Task HandlePaymentFailedAsync(Guid bookingId)
    {
        if (!Bookings.TryGetValue(bookingId, out var booking))
            return;

        booking.Status = BookingStatus.Failed;
        booking.CancelledAt = DateTime.UtcNow;

        await _carServiceClient.ReleaseAsync(booking.CarId);
    }

    // Krav: gratis annullering op til 48 timer før, ellers 25% gebyr,
    // og ingen annullering efter afhentning.
    public async Task<BookingResult> CancelBookingAsync(Guid bookingId)
    {
        if (!Bookings.TryGetValue(bookingId, out var booking))
            return new BookingResult(false, "Booking not found.", null);

        if (!booking.CanBeCancelled)
            return new BookingResult(false, "Booking cannot be cancelled after pickup.", null);

        var now = DateTime.UtcNow;

        if (booking.IsWithinFreeCancellationWindow(now))
        {
            booking.Status = BookingStatus.CancelledFree;
            booking.CancellationFee = 0m;
        }
        else
        {
            booking.Status = BookingStatus.CancelledWithFee;
            booking.CancellationFee = booking.TotalPrice * 0.25m;
        }

        booking.CancelledAt = now;

        await _carServiceClient.ReleaseAsync(booking.CarId);

        return new BookingResult(true, null, booking);
    }

    // Personale: bekræft afhentning.
    public Task<BookingResult> ConfirmPickupAsync(Guid bookingId)
    {
        if (!Bookings.TryGetValue(bookingId, out var booking))
            return Task.FromResult(new BookingResult(false, "Booking not found.", null));

        if (booking.Status != BookingStatus.Confirmed)
            return Task.FromResult(new BookingResult(false, "Only a confirmed booking can be picked up.", null));

        booking.Status = BookingStatus.PickedUp;
        return Task.FromResult(new BookingResult(true, null, booking));
    }

    // Personale: registrer aflevering.
    public async Task<BookingResult> RegisterReturnAsync(Guid bookingId)
    {
        if (!Bookings.TryGetValue(bookingId, out var booking))
            return new BookingResult(false, "Booking not found.", null);

        if (booking.Status != BookingStatus.PickedUp)
            return new BookingResult(false, "Only a picked-up booking can be returned.", null);

        booking.Status = BookingStatus.Completed;
        await _carServiceClient.ReleaseAsync(booking.CarId);

        return new BookingResult(true, null, booking);
    }
}
