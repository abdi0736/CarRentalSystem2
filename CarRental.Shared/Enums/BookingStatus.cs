namespace CarRental.Shared.Enums;

// Hele booking-forløbet:
// Pending: oprettet, venter på betaling
// Confirmed: betaling gennemført
// CancelledFree: annulleret 48+ timer før afhentning (ingen gebyr)
// CancelledWithFee: annulleret senere (25% gebyr)
// PickedUp: bilen afhentet, kan ikke længere annulleres
// Completed: bilen afleveret igen
// Failed: betaling fejlede, booking annulleret automatisk
public enum BookingStatus
{
    Pending,
    Confirmed,
    CancelledFree,
    CancelledWithFee,
    PickedUp,
    Completed,
    Failed
}
