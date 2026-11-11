using CarRental.BookingService.Services;
using Microsoft.AspNetCore.Mvc;

namespace CarRental.BookingService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BookingController : ControllerBase
{
    private readonly IBookingService _bookingService;

    // Sat op via DI i Program.cs.
    public BookingController(IBookingService bookingService)
    {
        _bookingService = bookingService;
    }

    public record CreateBookingRequest(Guid CustomerId, Guid CarId, DateTime StartDate, DateTime EndDate);

    // Krav: booking af bil for en valgt periode.
    [HttpPost]
    public async Task<IActionResult> CreateBooking(CreateBookingRequest request)
    {
        var result = await _bookingService.CreateBookingAsync(
            request.CustomerId, request.CarId, request.StartDate, request.EndDate);

        return result.Success ? Ok(result.Booking) : BadRequest(result.Error);
    }

    // Krav: annullering af booking.
    [HttpPost("{bookingId:guid}/cancel")]
    public async Task<IActionResult> CancelBooking(Guid bookingId)
    {
        var result = await _bookingService.CancelBookingAsync(bookingId);
        return result.Success ? Ok(result.Booking) : BadRequest(result.Error);
    }

    // Kaldes af Payment Service efter vellykket betaling.
    [HttpPost("{bookingId:guid}/confirm")]
    public async Task<IActionResult> ConfirmBooking(Guid bookingId)
    {
        var result = await _bookingService.ConfirmBookingAsync(bookingId);
        return result.Success ? Ok(result.Booking) : BadRequest(result.Error);
    }

    // Krav: booking-historik.
    [HttpGet("history/{customerId:guid}")]
    public async Task<IActionResult> GetHistory(Guid customerId)
    {
        var history = await _bookingService.GetBookingHistoryAsync(customerId);
        return Ok(history);
    }

    // Personale: bekræft afhentning.
    [HttpPost("{bookingId:guid}/pickup")]
    public async Task<IActionResult> ConfirmPickup(Guid bookingId)
    {
        var result = await _bookingService.ConfirmPickupAsync(bookingId);
        return result.Success ? Ok(result.Booking) : BadRequest(result.Error);
    }

    // Personale: registrer aflevering.
    [HttpPost("{bookingId:guid}/return")]
    public async Task<IActionResult> RegisterReturn(Guid bookingId)
    {
        var result = await _bookingService.RegisterReturnAsync(bookingId);
        return result.Success ? Ok(result.Booking) : BadRequest(result.Error);
    }
}
