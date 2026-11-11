using CarRental.PaymentService.Services;
using Microsoft.AspNetCore.Mvc;

namespace CarRental.PaymentService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymentController : ControllerBase
{
    private readonly IPaymentService _paymentService;

    public PaymentController(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    public record ProcessPaymentRequest(Guid BookingId, decimal Amount);

    // Krav: betaling online.
    [HttpPost]
    public async Task<IActionResult> ProcessPayment(ProcessPaymentRequest request)
    {
        var payment = await _paymentService.ProcessPaymentAsync(request.BookingId, request.Amount);
        return Ok(payment);
    }

    [HttpGet("booking/{bookingId:guid}")]
    public async Task<IActionResult> GetByBooking(Guid bookingId)
    {
        var payment = await _paymentService.GetByBookingIdAsync(bookingId);
        return payment is null ? NotFound() : Ok(payment);
    }
}
