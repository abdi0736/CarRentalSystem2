using CarRental.BookingService.Services;
using Microsoft.AspNetCore.Mvc;

namespace CarRental.BookingService.Controllers;

// FALLBACK: Payment Service kalder denne over REST når RabbitMq:Enabled=false.
// Med RabbitMQ slået til håndteres det af Messaging/PaymentFailedConsumer.
[ApiController]
[Route("api/[controller]")]
public class PaymentEventsController : ControllerBase
{
    private readonly IBookingService _bookingService;

    public PaymentEventsController(IBookingService bookingService)
    {
        _bookingService = bookingService;
    }

    public record PaymentFailedRequest(Guid BookingId);

    // Krav: fejlet betaling annullerer reservationen.
    [HttpPost("payment-failed")]
    public async Task<IActionResult> PaymentFailed(PaymentFailedRequest request)
    {
        await _bookingService.HandlePaymentFailedAsync(request.BookingId);
        return Ok();
    }
}
