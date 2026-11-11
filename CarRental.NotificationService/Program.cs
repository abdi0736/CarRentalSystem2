using CarRental.NotificationService.Consumers;
using CarRental.NotificationService.Messaging;
using CarRental.NotificationService.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Ingen database registreret — krav siger ingen egen database.
builder.Services.AddSingleton<INotificationService, NotificationService>();
builder.Services.AddSingleton<INotificationSender, EmailNotificationSender>();
builder.Services.AddSingleton<INotificationSender, SmsNotificationSender>();

// Modtager BookingConfirmedEvent fra RabbitMQ når RabbitMq:Enabled=true.
// NotificationController (REST) virker stadig som fallback.
builder.Services.Configure<RabbitMqOptions>(builder.Configuration.GetSection(RabbitMqOptions.SectionName));
if (builder.Configuration.GetValue<bool>($"{RabbitMqOptions.SectionName}:Enabled"))
{
    builder.Services.AddSingleton<RabbitMqConnection>();
    builder.Services.AddHostedService<BookingConfirmedConsumer>();
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
