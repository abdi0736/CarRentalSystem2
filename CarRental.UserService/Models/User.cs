using CarRental.Shared.Enums;

namespace CarRental.UserService.Models;

// Krav: brugere kan registrere sig og logge ind (kunde/admin-roller).
// Basisklasse for de tre brugertyper (Customer, Admin, Employee).
public abstract class User
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string FullName { get; set; } = string.Empty;

    // Bruges som login — skal være unik (tjekkes i UserService.RegisterCustomerAsync).
    public string Email { get; set; } = string.Empty;

    // Gem aldrig kodeordet i klartekst — se HashPassword i UserService.cs.
    public string PasswordHash { get; set; } = string.Empty;

    // Sættes af den enkelte underklasses konstruktør, derfor "protected" set.
    public UserRole Role { get; protected set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

// Krav: kunden skal være 21+ og have gyldigt kørekort.
public class Customer : User
{
    public Customer()
    {
        Role = UserRole.Customer;
    }

    public DateTime DateOfBirth { get; set; }
    public string DriversLicenceNumber { get; set; } = string.Empty;
    public DateTime DriversLicenceExpiry { get; set; }

    // Beregnes ud fra DateOfBirth, så den altid er korrekt.
    public int Age => CalculateAge(DateOfBirth);

    private static int CalculateAge(DateTime dateOfBirth)
    {
        var today = DateTime.UtcNow;
        var age = today.Year - dateOfBirth.Year;
        if (dateOfBirth.Date > today.AddYears(-age)) age--;
        return age;
    }

    // Gyldigt kun hvis der er et kørekortnummer, og det ikke er udløbet.
    public bool HasValidLicence => !string.IsNullOrWhiteSpace(DriversLicenceNumber)
                                    && DriversLicenceExpiry > DateTime.UtcNow;

    // Krav: 21+ OG gyldigt kørekort.
    public bool IsEligibleToRent => Age >= 21 && HasValidLicence;
}

// Krav: admin administrerer flåde og alle bookinger.
public class Admin : User
{
    public Admin()
    {
        Role = UserRole.Admin;
    }
}

// Krav: medarbejder står for afhentning/aflevering.
public class Employee : User
{
    public Employee()
    {
        Role = UserRole.Employee;
    }

    public string AssignedLocation { get; set; } = string.Empty;
}
