using CarRental.UserService.Models;
using CarRental.UserService.Services;
using Microsoft.AspNetCore.Mvc;

namespace CarRental.UserService.Controllers;

// Krav: registrering og login (kunde/admin-roller).
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IUserService _userService;

    // Sat op via DI i Program.cs.
    public AuthController(IUserService userService)
    {
        _userService = userService;
    }

    // Det frontend sender ved registrering — de sidste tre felter bruges
    // senere til alder/kørekort-tjek.
    public record RegisterCustomerRequest(
        string FullName,
        string Email,
        string Password,
        DateTime DateOfBirth,
        string DriversLicenceNumber,
        DateTime DriversLicenceExpiry);

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterCustomerRequest request)
    {
        var customer = new Customer
        {
            FullName = request.FullName,
            Email = request.Email,
            DateOfBirth = request.DateOfBirth,
            DriversLicenceNumber = request.DriversLicenceNumber,
            DriversLicenceExpiry = request.DriversLicenceExpiry
        };

        var result = await _userService.RegisterCustomerAsync(customer, request.Password);

        if (result is null)
            return Conflict("A user with this email already exists.");

        return CreatedAtAction(nameof(Register), new { id = result.Id }, result);
    }

    public record LoginRequest(string Email, string Password);

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var user = await _userService.LoginAsync(request.Email, request.Password);

        if (user is null)
            return Unauthorized("Invalid email or password.");

        // TODO: udsted JWT her når auth er sat op.
        return Ok(new { user.Id, user.FullName, user.Role });
    }
}
