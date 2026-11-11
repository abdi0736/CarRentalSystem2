using System.Text.Json;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;

namespace CarRental.PaymentService.Messaging;

// Én delt forbindelse til RabbitMQ pr. service (registreres som singleton).
// Channels er billige og ikke trådsikre, så der oprettes en ny pr. brug.
public sealed class RabbitMqConnection : IDisposable
{
    private const int MaxConnectAttempts = 10;
    private static readonly TimeSpan RetryDelay = TimeSpan.FromSeconds(3);

    public static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly RabbitMqOptions _options;
    private readonly ILogger<RabbitMqConnection> _logger;
    private readonly object _lock = new();
    private IConnection? _connection;

    public RabbitMqConnection(IOptions<RabbitMqOptions> options, ILogger<RabbitMqConnection> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public string Exchange => _options.Exchange;

    // Opretter en channel og sikrer at den fælles exchange findes.
    public IModel CreateChannel()
    {
        var channel = GetConnection().CreateModel();
        channel.ExchangeDeclare(_options.Exchange, ExchangeType.Topic, durable: true, autoDelete: false);
        return channel;
    }

    private IConnection GetConnection()
    {
        if (_connection is { IsOpen: true })
            return _connection;

        lock (_lock)
        {
            if (_connection is { IsOpen: true })
                return _connection;

            var factory = new ConnectionFactory
            {
                HostName = _options.Host,
                Port = _options.Port,
                UserName = _options.Username,
                Password = _options.Password,
                DispatchConsumersAsync = true,
                AutomaticRecoveryEnabled = true
            };

            // Brokeren kan være ved at starte op (fx lige efter docker compose up),
            // så der prøves igen et par gange før vi giver op.
            for (var attempt = 1; ; attempt++)
            {
                try
                {
                    _connection = factory.CreateConnection("CarRental.PaymentService");
                    _logger.LogInformation("Connected to RabbitMQ at {Host}:{Port}", _options.Host, _options.Port);
                    return _connection;
                }
                catch (BrokerUnreachableException ex) when (attempt < MaxConnectAttempts)
                {
                    _logger.LogWarning(ex, "RabbitMQ not reachable (attempt {Attempt}/{Max}), retrying in {Delay}s",
                        attempt, MaxConnectAttempts, RetryDelay.TotalSeconds);
                    Thread.Sleep(RetryDelay);
                }
            }
        }
    }

    public void Dispose() => _connection?.Dispose();
}
