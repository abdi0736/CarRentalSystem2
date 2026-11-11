using CarRental.UserService.Services;

var builder = WebApplication.CreateBuilder(args);

// Registrerer controllers (AuthController, UserController).
builder.Services.AddControllers();

// Lader Swagger læse endpoint-info fra controllerne.
builder.Services.AddEndpointsApiExplorer();

// Genererer OpenAPI/Swagger-dokumentationen.
builder.Services.AddSwaggerGen();

// DI: giv den in-memory implementering når IUserService bliver bedt om.
builder.Services.AddSingleton<IUserService, UserService>();

builder.Services.AddHealthChecks();

var app = builder.Build();

// Swagger UI vises kun i Development.
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

// Auth-pipeline (endnu ikke koblet til rigtig login — se TODO i AuthController).
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");
app.Run();
