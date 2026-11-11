using System.Net.Http.Json;
using CarRental.Shared.DTOs;

namespace CarRental.BookingService.Services;

public class CarServiceClient : ICarServiceClient
{
    private readonly HttpClient _httpClient;

    // Named HttpClient "CarService" er sat op i Program.cs med adresse fra appsettings.
    public CarServiceClient(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient("CarService");
    }

    public async Task<CarAvailabilityResponseDto?> CheckAvailabilityAsync(Guid carId, DateTime start, DateTime end)
    {
        var response = await _httpClient.PostAsJsonAsync("api/car/availability", new CarAvailabilityRequestDto
        {
            CarId = carId,
            StartDate = start,
            EndDate = end
        });

        return response.IsSuccessStatusCode
            ? await response.Content.ReadFromJsonAsync<CarAvailabilityResponseDto>()
            : null;
    }

    public async Task<bool> TryReserveAsync(Guid carId)
    {
        var response = await _httpClient.PostAsync($"api/car/{carId}/reserve", content: null);
        return response.IsSuccessStatusCode;
    }

    public async Task ReleaseAsync(Guid carId)
    {
        await _httpClient.PostAsync($"api/car/{carId}/release", content: null);
    }
}
