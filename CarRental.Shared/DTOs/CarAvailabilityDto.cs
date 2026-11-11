namespace CarRental.Shared.DTOs;

// Bruges af Booking Service -> Car Service.
// Krav: realtidstjek af tilgængelighed, ingen double booking.
public class CarAvailabilityRequestDto
{
    public Guid CarId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}

public class CarAvailabilityResponseDto
{
    public Guid CarId { get; set; }
    public bool IsAvailable { get; set; }

    // Sendes med her, så Booking Service kan gemme prisen på selve bookingen.
    public decimal PricePerDay { get; set; }
}
