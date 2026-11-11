using CarRental.Shared.DTOs;

namespace CarRental.BookingService.Services;

// Kapsler de synkrone REST-kald til Car Service.
public interface ICarServiceClient
{
    Task<CarAvailabilityResponseDto?> CheckAvailabilityAsync(Guid carId, DateTime start, DateTime end);
    Task<bool> TryReserveAsync(Guid carId);
    Task ReleaseAsync(Guid carId);
}
