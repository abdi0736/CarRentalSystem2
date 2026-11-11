using CarRental.UserService.Models;
using Xunit;
using UserSvc = CarRental.UserService.Services.UserService;

namespace CarRental.UserService.Tests;

public class UserServiceTests
{
    private readonly UserSvc _sut = new();

    // Unik email pr. kunde, fordi UserService gemmer i en static liste.
    private static Customer NewCustomer(int age, bool validLicence = true) => new()
    {
        FullName = "Test Kunde",
        Email = $"{Guid.NewGuid()}@test.dk",
        DateOfBirth = DateTime.UtcNow.AddYears(-age).AddDays(-1),
        DriversLicenceNumber = "DK12345678",
        DriversLicenceExpiry = validLicence ? DateTime.UtcNow.AddYears(5) : DateTime.UtcNow.AddDays(-1)
    };

    [Fact]
    public async Task Eligibility_UnknownCustomer_NotEligible()
    {
        var result = await _sut.CheckEligibilityAsync(Guid.NewGuid());

        Assert.False(result.IsEligible);
        Assert.Equal("Customer not found", result.Reason);
    }

    [Fact]
    public async Task Eligibility_Under21_NotEligible()
    {
        var customer = await _sut.RegisterCustomerAsync(NewCustomer(age: 20), "pw");

        var result = await _sut.CheckEligibilityAsync(customer!.Id);

        Assert.False(result.IsEligible);
        Assert.Equal("Customer is under 21 years old", result.Reason);
    }

    [Fact]
    public async Task Eligibility_ExpiredLicence_NotEligible()
    {
        var customer = await _sut.RegisterCustomerAsync(NewCustomer(age: 30, validLicence: false), "pw");

        var result = await _sut.CheckEligibilityAsync(customer!.Id);

        Assert.False(result.IsEligible);
    }

    [Fact]
    public async Task Eligibility_21WithValidLicence_IsEligible()
    {
        var customer = await _sut.RegisterCustomerAsync(NewCustomer(age: 21), "pw");

        var result = await _sut.CheckEligibilityAsync(customer!.Id);

        Assert.True(result.IsEligible);
    }

    [Fact]
    public async Task Register_DuplicateEmail_ReturnsNull()
    {
        var first = NewCustomer(age: 30);
        await _sut.RegisterCustomerAsync(first, "pw");

        var duplicate = NewCustomer(age: 30);
        duplicate.Email = first.Email;

        Assert.Null(await _sut.RegisterCustomerAsync(duplicate, "pw"));
    }

    [Fact]
    public async Task Login_CorrectPassword_ReturnsUser_WrongPassword_ReturnsNull()
    {
        var customer = NewCustomer(age: 30);
        await _sut.RegisterCustomerAsync(customer, "hemmeligt");

        Assert.NotNull(await _sut.LoginAsync(customer.Email, "hemmeligt"));
        Assert.Null(await _sut.LoginAsync(customer.Email, "forkert"));
    }
}