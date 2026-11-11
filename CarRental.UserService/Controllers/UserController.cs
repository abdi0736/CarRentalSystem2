using CarRental.UserService.Services;
using Microsoft.AspNetCore.Mvc;

namespace CarRental.UserService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;

    // Sat op via DI i Program.cs.
    public UserController(IUserService userService)
    {
        _userService = userService;
    }

    // Krav: se egen booking-historik — bruges til profilopslag.
    [HttpGet("{customerId:guid}")]
    public async Task<IActionResult> GetCustomer(Guid customerId)
    {
        var customer = await _userService.GetCustomerByIdAsync(customerId);
        return customer is null ? NotFound() : Ok(customer);
    }

    // Krav: 21+ og gyldigt kørekort — kaldes af Booking Service.
    [HttpGet("{customerId:guid}/eligibility")]
    public async Task<IActionResult> CheckEligibility(Guid customerId)
    {
        var result = await _userService.CheckEligibilityAsync(customerId);
        return Ok(result);
    }
}
