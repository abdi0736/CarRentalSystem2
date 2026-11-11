using CarRental.PaymentService.Messaging;
using CarRental.PaymentService.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHttpClient("BookingService", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Services:BookingService"]!);
});

// Scoped (ikke singleton), fordi PaymentService afhænger af scoped klienter.
// Data ligger i et static dictionary, så det overlever stadig mellem requests.
builder.Services.AddScoped<IPaymentService, CarRental.PaymentService.Services.PaymentService>();
builder.Services.AddScoped<IBookingServiceClient, BookingServiceClient>();

// Fejlet betaling sendes som event via RabbitMQ når RabbitMq:Enabled=true,
// ellers over REST til Booking Service (gammel MIDLERTIDIG løsning).
builder.Services.Configure<RabbitMqOptions>(builder.Configuration.GetSection(RabbitMqOptions.SectionName));
if (builder.Configuration.GetValue<bool>($"{RabbitMqOptions.SectionName}:Enabled"))
{
    builder.Services.AddSingleton<RabbitMqConnection>();
    builder.Services.AddScoped<IPaymentEventPublisher, RabbitMqPaymentEventPublisher>();
}
else
{
    builder.Services.AddScoped<IPaymentEventPublisher, HttpPaymentEventPublisher>();
}

builder.Services.AddHealthChecks();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// I containere kører vi kun HTTP på det interne netværk (ingen certifikat),
// så HTTPS-redirect slås kun til når vi IKKE kører i Docker.
if (!app.Configuration.GetValue<bool>("DOTNET_RUNNING_IN_CONTAINER"))
{
    app.UseHttpsRedirection();
}
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health");
app.Run();
