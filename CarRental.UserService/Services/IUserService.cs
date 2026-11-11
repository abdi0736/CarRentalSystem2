using CarRental.Shared.DTOs;
using CarRental.UserService.Models;

namespace CarRental.UserService.Services;

public interface IUserService
{
    Task<Customer?> RegisterCustomerAsync(Customer customer, string plainTextPassword);
    Task<User?> LoginAsync(string email, string plainTextPassword);
    Task<Customer?> GetCustomerByIdAsync(Guid customerId);
    Task<List<Customer>> GetAllCustomersAsync();

    // Krav: 21+ og gyldigt kørekort — kaldes af Booking Service før en booking oprettes.
    Task<CustomerEligibilityDto> CheckEligibilityAsync(Guid customerId);
}
