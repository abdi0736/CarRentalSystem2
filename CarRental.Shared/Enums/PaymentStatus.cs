namespace CarRental.Shared.Enums;

// Krav: hvis betaling fejler, annulleres reservationen.
public enum PaymentStatus
{
    Pending,     // ikke behandlet endnu
    Succeeded,   // godkendt -> booking bekræftes
    Failed,      // afvist -> booking annulleres
    Refunded     // pengene tilbagebetalt
}
