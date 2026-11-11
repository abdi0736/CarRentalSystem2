namespace CarRental.Shared.Enums;

// Krav: systemet skal tjekke tilgængelighed i realtid.
public enum CarAvailabilityStatus
{
    Available,      // kan søges og bookes
    Reserved,       // booket, venter på afhentning
    RentedOut,      // afhentet, ude hos kunden
    InMaintenance   // taget ud af flåden af admin, ikke bookbar
}
