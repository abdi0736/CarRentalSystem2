namespace CarRental.Shared.Events;

// Krav: bekræftelse sendes (email/SMS), og fejlet afsendelse må ikke
// annullere bookingen (der prøves igen i stedet).
// Sendes fra Booking Service til Notification Service via message queue,
// så en langsom/fejlende Notification Service aldrig kan blokere en booking.
public class BookingConfirmedEvent
{
    public Guid BookingId { get; set; }
    public Guid CustomerId { get; set; }

    // Bevidst tom herfra — Notification Service slår selv adressen op via User Service.
    public string CustomerEmail { get; set; } = string.Empty;

    public Guid CarId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal TotalPrice { get; set; }
}
