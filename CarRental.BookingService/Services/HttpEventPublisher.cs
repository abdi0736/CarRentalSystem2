using System.Net.Http.Json;
using CarRental.Shared.Events;

namespace CarRental.BookingService.Services;

// FALLBACK: kalder Notification Service direkte over REST. Bruges kun når
// RabbitMq:Enabled=false (fx når servicen køres alene fra IDE'en).
// I docker-compose bruges Messaging/RabbitMqEventPublisher i stedet.
public class HttpEventPublisher : IEventPublisher
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<HttpEventPublisher> _logger;

    public HttpEventPublisher(IHttpClientFactory httpClientFactory, ILogger<HttpEventPublisher> logger)
    {
        _httpClient = httpClientFactory.CreateClient("NotificationService");
        _logger = logger;
    }

    public async Task PublishBookingConfirmedAsync(BookingConfirmedEvent bookingConfirmed)
    {
        try
        {
            // En fejlende Notification Service må aldrig påvirke en bekræftet booking.
            await _httpClient.PostAsJsonAsync("api/notification/booking-confirmed", bookingConfirmed);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not reach Notification Service for booking {BookingId}", bookingConfirmed.BookingId);
        }
    }
}
