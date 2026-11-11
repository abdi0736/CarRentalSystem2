using CarRental.CarService.Models;
using CarRental.Shared.Enums;
using Xunit;
using CarSvc = CarRental.CarService.Services.CarService;

namespace CarRental.BookingService.Tests;

public class CarServiceTests
{
    private readonly CarSvc _sut = new();

    // Unik lokation pr. bil, så tests ikke ser hinandens biler (static dictionary).
    private async Task<Car> AddCarAsync(string? location = null, CarType type = CarType.Economy,
        int seats = 5, decimal price = 300m)
    {
        return await _sut.AddCarAsync(new Car
        {
            Make = "Test",
            Model = "Car",
            Type = type,
            PassengerCapacity = seats,
            PricePerDay = price,
            Location = location ?? Guid.NewGuid().ToString()
        });
    }

    [Fact]
    public async Task TryReserve_AvailableCar_FirstWins_SecondFails()
    {
        var car = await AddCarAsync();

        Assert.True(await _sut.TryReserveAsync(car.Id));
        Assert.False(await _sut.TryReserveAsync(car.Id));
    }

    [Fact]
    public async Task TryReserve_UnknownCar_ReturnsFalse()
    {
        Assert.False(await _sut.TryReserveAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task TryReserve_ManyParallelRequests_OnlyOneWins()
    {
        // Krav: aldrig double booking, først-til-mølle.
        var car = await AddCarAsync();

        var tasks = Enumerable.Range(0, 50).Select(_ => Task.Run(() => _sut.TryReserveAsync(car.Id)));
        var results = await Task.WhenAll(tasks);

        Assert.Single(results, r => r);
    }

    [Fact]
    public async Task Release_MakesCarAvailableAgain()
    {
        var car = await AddCarAsync();
        await _sut.TryReserveAsync(car.Id);

        await _sut.ReleaseAsync(car.Id);

        Assert.True(await _sut.TryReserveAsync(car.Id));
    }

    [Fact]
    public async Task Search_FiltersByLocationAndType()
    {
        var location = Guid.NewGuid().ToString();
        await AddCarAsync(location, CarType.Economy);
        var suv = await AddCarAsync(location, CarType.SUV);

        var result = await _sut.SearchAsync(location, CarType.SUV, null, null);

        var found = Assert.Single(result);
        Assert.Equal(suv.Id, found.Id);
    }

    [Fact]
    public async Task Search_DoesNotReturnReservedCars()
    {
        var location = Guid.NewGuid().ToString();
        var car = await AddCarAsync(location);
        await _sut.TryReserveAsync(car.Id);

        var result = await _sut.SearchAsync(location, null, null, null);

        Assert.Empty(result);
    }

    [Fact]
    public async Task AiRecommendations_RespectPassengersAndMaxPrice()
    {
        var bigCheap = await AddCarAsync(seats: 7, price: 400m);
        var tooSmall = await AddCarAsync(seats: 4, price: 200m);
        var tooExpensive = await AddCarAsync(seats: 7, price: 900m);

        var result = await _sut.GetAiRecommendationsAsync(passengerCount: 6, maxPricePerDay: 500m);

        Assert.Contains(result, c => c.Id == bigCheap.Id);
        Assert.DoesNotContain(result, c => c.Id == tooSmall.Id);
        Assert.DoesNotContain(result, c => c.Id == tooExpensive.Id);
    }
}