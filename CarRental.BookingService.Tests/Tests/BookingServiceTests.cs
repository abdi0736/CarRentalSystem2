using CarRental.Shared.Enums;
using Xunit;
using BookingSvc = CarRental.BookingService.Services.BookingService;

namespace CarRental.BookingService.Tests;

public class BookingServiceTests
{
    private readonly FakeCarServiceClient _car = new();
    private readonly FakeUserServiceClient _user = new();
    private readonly FakeEventPublisher _events = new();
    private readonly BookingSvc _sut;

    // xUnit laver en ny instans pr. test, så fakes er friske hver gang.
    public BookingServiceTests()
    {
        _sut = new BookingSvc(_car, _user, _events);
    }

    // Ny bil-id pr. test, fordi BookingService gemmer i en static dictionary.
    private static Guid NewCar() => Guid.NewGuid();
    private static Guid NewCustomer() => Guid.NewGuid();

    [Fact]
    public async Task CreateBooking_EndDateBeforeStartDate_Fails()
    {
        var start = DateTime.UtcNow.AddDays(5);

        var result = await _sut.CreateBookingAsync(NewCustomer(), NewCar(), start, start.AddDays(-1));

        Assert.False(result.Success);
    }

    [Fact]
    public async Task CreateBooking_ValidRequest_CreatesPendingBookingWithCorrectPrice()
    {
        _car.PricePerDay = 500m;
        var start = DateTime.UtcNow.AddDays(5);

        var result = await _sut.CreateBookingAsync(NewCustomer(), NewCar(), start, start.AddDays(3));

        Assert.True(result.Success);
        Assert.Equal(BookingStatus.Pending, result.Booking!.Status);
        Assert.Equal(1500m, result.Booking.TotalPrice); // 3 dage * 500
    }

    [Fact]
    public async Task CreateBooking_CustomerNotEligible_Fails()
    {
        _user.IsEligible = false;
        _user.Reason = "Customer is under 21 years old";
        var start = DateTime.UtcNow.AddDays(5);

        var result = await _sut.CreateBookingAsync(NewCustomer(), NewCar(), start, start.AddDays(2));

        Assert.False(result.Success);
        Assert.Equal("Customer is under 21 years old", result.Error);
    }

    [Fact]
    public async Task CreateBooking_CarNotAvailable_Fails()
    {
        _car.IsAvailable = false;
        var start = DateTime.UtcNow.AddDays(5);

        var result = await _sut.CreateBookingAsync(NewCustomer(), NewCar(), start, start.AddDays(2));

        Assert.False(result.Success);
    }

    [Fact]
    public async Task CreateBooking_CarReservedByAnotherCustomer_Fails()
    {
        _car.ReserveSucceeds = false; // en anden nåede det først
        var start = DateTime.UtcNow.AddDays(5);

        var result = await _sut.CreateBookingAsync(NewCustomer(), NewCar(), start, start.AddDays(2));

        Assert.False(result.Success);
    }

    [Fact]
    public async Task CreateBooking_OverlappingPeriodOnSameCar_Fails()
    {
        var carId = NewCar();
        var start = DateTime.UtcNow.AddDays(10);
        await _sut.CreateBookingAsync(NewCustomer(), carId, start, start.AddDays(3));

        var second = await _sut.CreateBookingAsync(NewCustomer(), carId, start.AddDays(1), start.AddDays(4));

        Assert.False(second.Success);
    }

    [Fact]
    public async Task CreateBooking_NonOverlappingPeriodOnSameCar_Succeeds()
    {
        var carId = NewCar();
        var start = DateTime.UtcNow.AddDays(10);
        await _sut.CreateBookingAsync(NewCustomer(), carId, start, start.AddDays(3));

        var second = await _sut.CreateBookingAsync(NewCustomer(), carId, start.AddDays(5), start.AddDays(7));

        Assert.True(second.Success);
    }

    [Fact]
    public async Task Cancel_MoreThan48HoursBefore_IsFree_AndReleasesCar()
    {
        var carId = NewCar();
        var start = DateTime.UtcNow.AddDays(5);
        var booking = (await _sut.CreateBookingAsync(NewCustomer(), carId, start, start.AddDays(2))).Booking!;

        var result = await _sut.CancelBookingAsync(booking.Id);

        Assert.True(result.Success);
        Assert.Equal(BookingStatus.CancelledFree, result.Booking!.Status);
        Assert.Equal(0m, result.Booking.CancellationFee);
        Assert.Contains(carId, _car.ReleasedCars);
    }

    [Fact]
    public async Task Cancel_LessThan48HoursBefore_Costs25Percent()
    {
        _car.PricePerDay = 400m;
        var start = DateTime.UtcNow.AddHours(24);
        var booking = (await _sut.CreateBookingAsync(NewCustomer(), NewCar(), start, start.AddDays(2))).Booking!;

        var result = await _sut.CancelBookingAsync(booking.Id);

        Assert.True(result.Success);
        Assert.Equal(BookingStatus.CancelledWithFee, result.Booking!.Status);
        Assert.Equal(200m, result.Booking.CancellationFee); // 25% af 800
    }

    [Fact]
    public async Task Cancel_AfterPickup_Fails()
    {
        var start = DateTime.UtcNow.AddDays(5);
        var booking = (await _sut.CreateBookingAsync(NewCustomer(), NewCar(), start, start.AddDays(2))).Booking!;
        await _sut.ConfirmBookingAsync(booking.Id);
        await _sut.ConfirmPickupAsync(booking.Id);

        var result = await _sut.CancelBookingAsync(booking.Id);

        Assert.False(result.Success);
    }

    [Fact]
    public async Task ConfirmBooking_PublishesBookingConfirmedEvent()
    {
        var start = DateTime.UtcNow.AddDays(5);
        var booking = (await _sut.CreateBookingAsync(NewCustomer(), NewCar(), start, start.AddDays(2))).Booking!;

        var result = await _sut.ConfirmBookingAsync(booking.Id);

        Assert.True(result.Success);
        Assert.Equal(BookingStatus.Confirmed, result.Booking!.Status);
        var evt = Assert.Single(_events.Published);
        Assert.Equal(booking.Id, evt.BookingId);
    }

    [Fact]
    public async Task PaymentFailed_SetsBookingToFailed_AndReleasesCar()
    {
        var carId = NewCar();
        var start = DateTime.UtcNow.AddDays(5);
        var booking = (await _sut.CreateBookingAsync(NewCustomer(), carId, start, start.AddDays(2))).Booking!;

        await _sut.HandlePaymentFailedAsync(booking.Id);

        Assert.Equal(BookingStatus.Failed, booking.Status);
        Assert.Contains(carId, _car.ReleasedCars);
    }
}