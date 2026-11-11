namespace CarRental.PaymentService.Messaging;

// Bindes til "RabbitMq"-sektionen i appsettings.json / miljøvariabler
// (fx RabbitMq__Host=rabbitmq i docker-compose).
public class RabbitMqOptions
{
    public const string SectionName = "RabbitMq";

    // false = brug den gamle REST-løsning (så servicen kan køre uden broker lokalt).
    public bool Enabled { get; set; }

    public string Host { get; set; } = "localhost";
    public int Port { get; set; } = 5672;
    public string Username { get; set; } = "guest";
    public string Password { get; set; } = "guest";

    // Fælles topic exchange som alle events publiceres på.
    public string Exchange { get; set; } = "carrental.events";
}

// Routing keys deles via konvention (ikke via Shared-pakken), så messaging
// kan ændres uden at bumpe CarRental.Shared.
public static class RoutingKeys
{
    public const string BookingConfirmed = "booking.confirmed";
    public const string PaymentFailed = "payment.failed";
}
