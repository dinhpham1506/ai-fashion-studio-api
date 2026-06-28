var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

// Health + ping (used by docker-compose / smoke tests)
app.MapGet("/health", () => Results.Ok(new { service = "dotnet-platform-api", status = "UP" }));
app.MapGet("/api/ping", () => Results.Ok(new { service = "dotnet-platform-api", status = "UP" }));

app.MapControllers();

app.Run();
