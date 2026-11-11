using CarRental.BookingService.Messaging;
using CarRental.BookingService.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Adresser hentes fra appsettings.json ("Services:CarService" osv.).
builder.Services.AddHttpClient("CarService", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Services:CarService"]!);
});

builder.Services.AddHttpClient("UserService", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Services:UserService"]!);
});

builder.Services.AddHttpClient("NotificationService", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Services:NotificationService"]!);
});

// Scoped (ikke singleton), fordi BookingService afhænger af scoped HTTP-klienter.
// Data ligger i et static dictionary, så det overlever stadig mellem requests.
builder.Services.AddScoped<IBookingService, CarRental.BookingService.Services.BookingService>();
builder.Services.AddScoped<ICarServiceClient, CarServiceClient>();
builder.Services.AddScoped<IUserServiceClient, UserServiceClient>();

// Asynkron kommunikation via RabbitMQ når RabbitMq:Enabled=true (docker-compose).
// Ellers falder vi tilbage til den gamle REST-løsning, så servicen kan køres
// alene fra IDE'en uden en broker.
builder.Services.Configure<RabbitMqOptions>(builder.Configuration.GetSection(RabbitMqOptions.SectionName));
if (builder.Configuration.GetValue<bool>($"{RabbitMqOptions.SectionName}:Enabled"))
{
    builder.Services.AddSingleton<RabbitMqConnection>();
    builder.Services.AddScoped<IEventPublisher, RabbitMqEventPublisher>();
    builder.Services.AddHostedService<PaymentFailedConsumer>();
}
else
{
    builder.Services.AddScoped<IEventPublisher, HttpEventPublisher>();
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
