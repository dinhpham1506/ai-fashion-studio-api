var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// OpenAPI/Swagger (enabled for all environments so it is testable)
app.UseSwagger();
app.UseSwaggerUI();

// Health + ping (used by docker-compose / smoke tests)
app.MapGet("/health", () => Results.Ok(new { service = "dotnet-platform-api", status = "UP" }));
app.MapGet("/api/ping", () => Results.Ok(new { service = "dotnet-platform-api", status = "UP" }));

app.MapControllers();

app.Run();
