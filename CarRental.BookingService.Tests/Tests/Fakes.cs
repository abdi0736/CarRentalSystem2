using CarRental.BookingService.Services;
using CarRental.Shared.DTOs;
using CarRental.Shared.Events;

namespace CarRental.BookingService.Tests;

// Erstatter REST-kaldet til Car Service i tests.
public class FakeCarServiceClient : ICarServiceClient
{
    public bool IsAvailable { get; set; } = true;
    public decimal PricePerDay { get; set; } = 500m;
    public bool ReserveSucceeds { get; set; } = true;
    public List<Guid> ReleasedCars { get; } = new();

    public Task<CarAvailabilityResponseDto?> CheckAvailabilityAsync(Guid carId, DateTime start, DateTime end)
        => Task.FromResult<CarAvailabilityResponseDto?>(new CarAvailabilityResponseDto
        {
            CarId = carId,
            IsAvailable = IsAvailable,
            PricePerDay = PricePerDay
        });

    public Task<bool> TryReserveAsync(Guid carId) => Task.FromResult(ReserveSucceeds);

    public Task ReleaseAsync(Guid carId)
    {
        ReleasedCars.Add(carId);
        return Task.CompletedTask;
    }
}

// Erstatter REST-kaldet til User Service i tests.
public class FakeUserServiceClient : IUserServiceClient
{
    public bool IsEligible { get; set; } = true;
    public string? Reason { get; set; }

    public Task<CustomerEligibilityDto?> CheckEligibilityAsync(Guid customerId)
        => Task.FromResult<CustomerEligibilityDto?>(new CustomerEligibilityDto
        {
            CustomerId = customerId,
            IsEligible = IsEligible,
            Reason = Reason
        });
}

// Erstatter RabbitMQ i tests: gemmer bare de events, der ville være sendt.
public class FakeEventPublisher : IEventPublisher
{
    public List<BookingConfirmedEvent> Published { get; } = new();

    public Task PublishBookingConfirmedAsync(BookingConfirmedEvent bookingConfirmed)
    {
        Published.Add(bookingConfirmed);
        return Task.CompletedTask;
    }
}