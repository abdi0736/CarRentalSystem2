using CarRental.CarService.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// DI: giv den in-memory implementering når ICarService bliver bedt om.
builder.Services.AddSingleton<ICarService, CarService>();

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
