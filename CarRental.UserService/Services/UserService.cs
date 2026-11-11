using System.Security.Cryptography;
using System.Text;
using CarRental.Shared.DTOs;
using CarRental.UserService.Models;

namespace CarRental.UserService.Services;

// NB: gemmer i memory for nu — skiftes til EF Core-database senere,
// uden at IUserService-interfacet skal ændres.
public class UserService : IUserService
{
    private static readonly List<Customer> Customers = new();
    private static readonly List<Admin> Admins = new();
    private static readonly List<Employee> Employees = new();

    // Krav: registrering.
    public Task<Customer?> RegisterCustomerAsync(Customer customer, string plainTextPassword)
    {
        if (Customers.Any(c => c.Email == customer.Email))
            return Task.FromResult<Customer?>(null); // email allerede i brug

        customer.PasswordHash = HashPassword(plainTextPassword);
        Customers.Add(customer);
        return Task.FromResult<Customer?>(customer);
    }

    // Krav: login — tjekker alle tre rolle-lister.
    public Task<User?> LoginAsync(string email, string plainTextPassword)
    {
        var hash = HashPassword(plainTextPassword);

        User? user = Customers.FirstOrDefault(c => c.Email == email && c.PasswordHash == hash)
                      ?? (User?)Admins.FirstOrDefault(a => a.Email == email && a.PasswordHash == hash)
                      ?? Employees.FirstOrDefault(e => e.Email == email && e.PasswordHash == hash);

        return Task.FromResult(user);
    }

    public Task<Customer?> GetCustomerByIdAsync(Guid customerId)
    {
        return Task.FromResult(Customers.FirstOrDefault(c => c.Id == customerId));
    }

    public Task<List<Customer>> GetAllCustomersAsync()
    {
        return Task.FromResult(Customers);
    }

    // Krav: eligibility-tjek som Booking Service kalder før booking oprettes.
    public Task<CustomerEligibilityDto> CheckEligibilityAsync(Guid customerId)
    {
        var customer = Customers.FirstOrDefault(c => c.Id == customerId);

        if (customer is null)
        {
            return Task.FromResult(new CustomerEligibilityDto
            {
                CustomerId = customerId,
                IsEligible = false,
                Reason = "Customer not found"
            });
        }

        if (!customer.IsEligibleToRent)
        {
            var reason = customer.Age < 21
                ? "Customer is under 21 years old"
                : "Customer does not have a valid driver's licence";

            return Task.FromResult(new CustomerEligibilityDto
            {
                CustomerId = customerId,
                IsEligible = false,
                Reason = reason
            });
        }

        return Task.FromResult(new CustomerEligibilityDto
        {
            CustomerId = customerId,
            IsEligible = true
        });
    }

    // Midlertidig simpel hashing — skift til ASP.NET Identity/BCrypt før produktion.
    private static string HashPassword(string password)
    {
        var bytes = Encoding.UTF8.GetBytes(password);
        var hash = SHA256.HashData(bytes);
        return Convert.ToBase64String(hash);
    }
}
