namespace CarRental.Shared.Enums;

// Krav: brugere kan registrere sig og logge ind (kunde og admin-roller).
// Employee er tilføjet pga. "Employee at pickup and drop-off" i opgaven.
public enum UserRole
{
    Customer,   // lejer biler, eneste rolle med alder/kørekort-tjek
    Admin,      // administrerer flåde og alle bookinger
    Employee    // står for afhentning/aflevering
}
