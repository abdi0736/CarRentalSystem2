using CarRental.NotificationService.Services;
using CarRental.Shared.Events;
using Microsoft.AspNetCore.Mvc;

namespace CarRental.NotificationService.Controllers;

// FALLBACK: Booking Service kalder denne over REST når RabbitMq:Enabled=false.
// Med RabbitMQ slået til håndteres det af Consumers/BookingConfirmedConsumer.
[ApiController]
[Route("api/[controller]")]
public class NotificationController : ControllerBase
{
    private readonly INotificationService _notificationService;

    public NotificationController(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    // Krav: bekræftelse sendes via email/SMS.
    [HttpPost("booking-confirmed")]
    public async Task<IActionResult> BookingConfirmed(BookingConfirmedEvent bookingConfirmed)
    {
        await _notificationService.NotifyBookingConfirmedAsync(bookingConfirmed);
        return Ok();
    }
}
