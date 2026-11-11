namespace CarRental.Shared.Events;

// Krav: hvis betaling fejler, annulleres reservationen (saga-kompensation).
// NB: den nuværende midlertidige REST-løsning sender kun BookingId direkte
// i stedet for hele denne event — denne klasse er klar til rigtig messaging.
public class PaymentFailedEvent
{
    public Guid BookingId { get; set; }
    public Guid PaymentId { get; set; }
    public string Reason { get; set; } = string.Empty;
}
