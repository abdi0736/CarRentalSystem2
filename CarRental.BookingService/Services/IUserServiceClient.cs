using CarRental.Shared.DTOs;

namespace CarRental.BookingService.Services;

public interface IUserServiceClient
{
    // Krav: 21+ og gyldigt kørekort.
    Task<CustomerEligibilityDto?> CheckEligibilityAsync(Guid customerId);
}
