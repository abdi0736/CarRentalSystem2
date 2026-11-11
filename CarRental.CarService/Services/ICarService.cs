using CarRental.CarService.Models;
using CarRental.Shared.DTOs;
using CarRental.Shared.Enums;

namespace CarRental.CarService.Services;

public interface ICarService
{
    Task<List<Car>> SearchAsync(string? location, CarType? type, DateTime? startDate, DateTime? endDate);

    Task<Car> AddCarAsync(Car car);
    Task<Car?> UpdateCarAsync(Car car);
    Task<bool> RemoveCarAsync(Guid carId);

    Task<CarAvailabilityResponseDto> CheckAvailabilityAsync(CarAvailabilityRequestDto request);

    // Krav: først-til-mølle ved konkurrerende bookinger, aldrig double booking.
    // Sætter status til Reserved atomisk, kun hvis den stadig er Available.
    Task<bool> TryReserveAsync(Guid carId);

    Task ReleaseAsync(Guid carId);

    // Krav: AI-anbefaling ud fra antal passagerer og prisinterval.
    Task<List<Car>> GetAiRecommendationsAsync(int passengerCount, decimal maxPricePerDay);
}
