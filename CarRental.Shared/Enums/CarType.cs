namespace CarRental.Shared.Enums;

// Krav: søgning/filtrering på biltype, og pris pr. dag afhænger af biltype.
public enum CarType
{
    Economy,    // billigste, mindste biler
    Compact,
    SUV,
    Luxury,     // dyreste
    Van         // flest passagerer, til gruppebookinger
}
