namespace CarRental.Shared.DTOs;

// Bruges af Booking Service -> User Service.
// Krav: kunden skal være 21+ med gyldigt kørekort.
public class CustomerEligibilityDto
{
    public Guid CustomerId { get; set; }
    public bool IsEligible { get; set; }

    // Forklaring når IsEligible er false, vises til kunden.
    public string? Reason { get; set; }
}
