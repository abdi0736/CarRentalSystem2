using System.Collections.Concurrent;
using CarRental.CarService.Models;
using CarRental.Shared.DTOs;
using CarRental.Shared.Enums;

namespace CarRental.CarService.Services;

// NB: gemmer i memory for nu — skiftes til EF Core-database senere.
public class CarService : ICarService
{
    private static readonly ConcurrentDictionary<Guid, Car> Cars = new();

    // Én lock pr. bil, så tjek på uafhængige biler ikke blokerer hinanden,
    // men samme bil aldrig kan dobbeltbookes.
    private static readonly ConcurrentDictionary<Guid, object> CarLocks = new();

    // Krav: søgning/filtrering på lokation, dato, biltype.
    public Task<List<Car>> SearchAsync(string? location, CarType? type, DateTime? startDate, DateTime? endDate)
    {
        var query = Cars.Values.Where(c => c.Status == CarAvailabilityStatus.Available);

        if (!string.IsNullOrWhiteSpace(location))
            query = query.Where(c => c.Location.Equals(location, StringComparison.OrdinalIgnoreCase));

        if (type.HasValue)
            query = query.Where(c => c.Type == type.Value);

        // start/slut-dato tjekkes ikke her endnu — det kræver et kald til Booking Service.

        return Task.FromResult(query.ToList());
    }

    // Krav: admin kan tilføje/opdatere/fjerne biler.
    public Task<Car> AddCarAsync(Car car)
    {
        Cars[car.Id] = car;
        return Task.FromResult(car);
    }

    public Task<Car?> UpdateCarAsync(Car car)
    {
        if (!Cars.ContainsKey(car.Id))
            return Task.FromResult<Car?>(null);

        Cars[car.Id] = car;
        return Task.FromResult<Car?>(car);
    }

    public Task<bool> RemoveCarAsync(Guid carId)
    {
        return Task.FromResult(Cars.TryRemove(carId, out _));
    }

    // Krav: realtidstjek af tilgængelighed.
    public Task<CarAvailabilityResponseDto> CheckAvailabilityAsync(CarAvailabilityRequestDto request)
    {
        Cars.TryGetValue(request.CarId, out var car);

        return Task.FromResult(new CarAvailabilityResponseDto
        {
            CarId = request.CarId,
            IsAvailable = car is not null && car.Status == CarAvailabilityStatus.Available,
            PricePerDay = car?.PricePerDay ?? 0m
        });
    }

    // Krav: først-til-mølle, aldrig double booking.
    // Lock + statustjek gør det atomisk, så to samtidige bookinger ikke begge kan vinde.
    public Task<bool> TryReserveAsync(Guid carId)
    {
        if (!Cars.TryGetValue(carId, out var car))
            return Task.FromResult(false);

        var carLock = CarLocks.GetOrAdd(carId, _ => new object());

        lock (carLock)
        {
            if (car.Status != CarAvailabilityStatus.Available)
                return Task.FromResult(false); // en anden nåede det først

            car.Status = CarAvailabilityStatus.Reserved;
            return Task.FromResult(true);
        }
    }

    // Kaldes af Booking Service ved annullering, fejlet betaling eller aflevering.
    public Task ReleaseAsync(Guid carId)
    {
        if (Cars.TryGetValue(carId, out var car))
        {
            var carLock = CarLocks.GetOrAdd(carId, _ => new object());
            lock (carLock)
            {
                car.Status = CarAvailabilityStatus.Available;
            }
        }

        return Task.CompletedTask;
    }

    // Simpel regelbaseret stand-in for AI-anbefalingen — skift senere til rigtigt kald.
    public Task<List<Car>> GetAiRecommendationsAsync(int passengerCount, decimal maxPricePerDay)
    {
        var recommendations = Cars.Values
            .Where(c => c.Status == CarAvailabilityStatus.Available)
            .Where(c => c.PassengerCapacity >= passengerCount)
            .Where(c => c.PricePerDay <= maxPricePerDay)
            .OrderBy(c => c.PricePerDay)
            .ToList();

        return Task.FromResult(recommendations);
    }
}
