using CarRental.CarService.Models;
using CarRental.CarService.Services;
using CarRental.Shared.DTOs;
using CarRental.Shared.Enums;
using Microsoft.AspNetCore.Mvc;

namespace CarRental.CarService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CarController : ControllerBase
{
    private readonly ICarService _carService;

    // Sat op via DI i Program.cs.
    public CarController(ICarService carService)
    {
        _carService = carService;
    }

    // Krav: søgning/filtrering på lokation, dato, biltype.
    [HttpGet("search")]
    public async Task<IActionResult> Search(
        [FromQuery] string? location,
        [FromQuery] CarType? type,
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate)
    {
        var cars = await _carService.SearchAsync(location, type, startDate, endDate);
        return Ok(cars);
    }

    // Krav: AI-anbefaling ud fra passagerer og prisinterval.
    [HttpGet("recommendations")]
    public async Task<IActionResult> GetRecommendations([FromQuery] int passengers, [FromQuery] decimal maxPrice)
    {
        var recommendations = await _carService.GetAiRecommendationsAsync(passengers, maxPrice);
        return Ok(recommendations);
    }

    // Kaldes af Booking Service.
    [HttpPost("availability")]
    public async Task<IActionResult> CheckAvailability(CarAvailabilityRequestDto request)
    {
        var result = await _carService.CheckAvailabilityAsync(request);
        return Ok(result);
    }

    // Kaldes af Booking Service når en booking oprettes.
    [HttpPost("{carId:guid}/reserve")]
    public async Task<IActionResult> Reserve(Guid carId)
    {
        var success = await _carService.TryReserveAsync(carId);
        return success ? Ok() : Conflict("Car is no longer available.");
    }

    // Kaldes af Booking Service ved annullering eller fejlet betaling.
    [HttpPost("{carId:guid}/release")]
    public async Task<IActionResult> Release(Guid carId)
    {
        await _carService.ReleaseAsync(carId);
        return Ok();
    }

    // --- Admin flådestyring ---
    // Krav: admin kan tilføje/opdatere/fjerne biler.

    [HttpPost]
    public async Task<IActionResult> AddCar(Car car)
    {
        var created = await _carService.AddCarAsync(car);
        return CreatedAtAction(nameof(Search), new { id = created.Id }, created);
    }

    [HttpPut("{carId:guid}")]
    public async Task<IActionResult> UpdateCar(Guid carId, Car car)
    {
        if (carId != car.Id)
            return BadRequest("Route id and body id must match.");

        var updated = await _carService.UpdateCarAsync(car);
        return updated is null ? NotFound() : Ok(updated);
    }

    [HttpDelete("{carId:guid}")]
    public async Task<IActionResult> RemoveCar(Guid carId)
    {
        var removed = await _carService.RemoveCarAsync(carId);
        return removed ? NoContent() : NotFound();
    }
}
