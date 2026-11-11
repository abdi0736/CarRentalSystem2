using CarRental.Shared.Enums;

namespace CarRental.BookingService.Models;

public class Booking
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid CustomerId { get; set; }
    public Guid CarId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }

    // Prisen på bookingtidspunktet, så en senere prisændring ikke rammer kunden.
    public decimal PricePerDay { get; set; }

    public BookingStatus Status { get; set; } = BookingStatus.Pending;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CancelledAt { get; set; }
    public decimal? CancellationFee { get; set; }

    // Altid mindst 1 dag, selv ved samme-dags leje.
    public int DurationInDays => Math.Max(1, (EndDate.Date - StartDate.Date).Days);

    // Krav: pris pr. dag efter biltype.
    public decimal TotalPrice => DurationInDays * PricePerDay;

    // Krav: gratis annullering op til 48 timer før afhentning, ellers 25% gebyr.
    public bool IsWithinFreeCancellationWindow(DateTime now) => (StartDate - now).TotalHours >= 48;

    // Krav: kan ikke annulleres efter afhentning.
    public bool CanBeCancelled => Status is BookingStatus.Pending or BookingStatus.Confirmed;
    
}
