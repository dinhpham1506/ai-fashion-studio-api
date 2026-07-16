using System.Text;
using AiFashionStudio.Platform.Application.Common.Interfaces.IServices;
using AiFashionStudio.Platform.Api.Middlewares;
using AiFashionStudio.Platform.Application;
using AiFashionStudio.Platform.Infrastructure;
using AiFashionStudio.Platform.Infrastructure.Identity;
using AiFashionStudio.Platform.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    // Nút Authorize trên Swagger UI: dán access token (không cần gõ chữ "Bearer")
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Nhập access token nhận được từ /api/auth/login"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// CORS cho frontend. Origin lấy từ config "Cors:AllowedOrigins" — thêm origin mới
// (ngrok, vercel...) chỉ cần sửa appsettings, không cần sửa code.
// AllowCredentials bắt buộc để browser chấp nhận cookie refresh_token khi cross-origin.
const string FrontendCorsPolicy = "Frontend";
var corsOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
    ?? new[] { "http://localhost:5173", "http://localhost:3000" };
builder.Services.AddCors(options =>
{
    options.AddPolicy(FrontendCorsPolicy, policy =>
        policy.WithOrigins(corsOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials());
});

var jwtSettings = builder.Configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>() ?? new JwtSettings();

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtSettings.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Secret)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30),
            // MapInboundClaims = false nên claim trong JWT giữ nguyên tên gốc ("role", "sub"...)
            // — phải chỉ rõ RoleClaimType, nếu không [Authorize(Roles=...)]/User.IsInRole không nhận role
            RoleClaimType = "role"
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
    dbContext.Database.ExecuteSqlRaw("""
        CREATE SCHEMA IF NOT EXISTS platform;

        CREATE TABLE IF NOT EXISTS platform."__EFMigrationsHistory" (
            "MigrationId" character varying(150) NOT NULL,
            "ProductVersion" character varying(32) NOT NULL,
            CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
        );

        DO $$
        BEGIN
            IF to_regclass('public."__EFMigrationsHistory"') IS NOT NULL THEN
                INSERT INTO platform."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
                SELECT "MigrationId", "ProductVersion"
                FROM public."__EFMigrationsHistory"
                ON CONFLICT ("MigrationId") DO NOTHING;

                DROP TABLE public."__EFMigrationsHistory";
            END IF;
        END $$;
        """);
    dbContext.Database.Migrate();

    var seedDemoData = app.Configuration.GetValue<bool>("Seeding:EnableDemoData");
    if (seedDemoData && !app.Environment.IsProduction())
    {
        await DatabaseSeeder.SeedAsync(dbContext, passwordHasher);
    }
}

// OpenAPI/Swagger (enabled for all environments so it is testable)
app.UseSwagger();
app.UseSwaggerUI();

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseCors(FrontendCorsPolicy);

app.UseAuthentication();
app.UseAuthorization();

// Health + ping (used by docker-compose / smoke tests)
app.MapGet("/health", () => Results.Ok(new { service = "dotnet-platform-api", status = "UP" }));
app.MapGet("/api/ping", () => Results.Ok(new { service = "dotnet-platform-api", status = "UP" }));

app.MapControllers();

app.Run();
