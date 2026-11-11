using System.Net.Http.Json;
using CarRental.Shared.DTOs;

namespace CarRental.BookingService.Services;

public class UserServiceClient : IUserServiceClient
{
    private readonly HttpClient _httpClient;

    // Named HttpClient "UserService" er sat op i Program.cs med adresse fra appsettings.
    public UserServiceClient(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient("UserService");
    }

    public async Task<CustomerEligibilityDto?> CheckEligibilityAsync(Guid customerId)
    {
        var response = await _httpClient.GetAsync($"api/user/{customerId}/eligibility");

        return response.IsSuccessStatusCode
            ? await response.Content.ReadFromJsonAsync<CustomerEligibilityDto>()
            : null;
    }
}
