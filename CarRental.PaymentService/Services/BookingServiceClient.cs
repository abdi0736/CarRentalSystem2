using System.Net.Http.Json;

namespace CarRental.PaymentService.Services;

public class BookingServiceClient : IBookingServiceClient
{
    private readonly HttpClient _httpClient;

    // Named HttpClient "BookingService" er sat op i Program.cs med adresse fra appsettings.
    public BookingServiceClient(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient("BookingService");
    }

    public async Task ConfirmBookingAsync(Guid bookingId)
    {
        await _httpClient.PostAsync($"api/booking/{bookingId}/confirm", content: null);
    }

    public async Task NotifyPaymentFailedAsync(Guid bookingId)
    {
        await _httpClient.PostAsJsonAsync("api/paymentevents/payment-failed", new { BookingId = bookingId });
    }
}
