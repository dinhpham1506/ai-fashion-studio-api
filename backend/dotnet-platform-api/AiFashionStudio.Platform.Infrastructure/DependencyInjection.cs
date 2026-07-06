using AiFashionStudio.Platform.Application.Common.Interfaces.IServices;
using AiFashionStudio.Platform.Application.Common.Interfaces.IRepositories;
using AiFashionStudio.Platform.Infrastructure.Identity;
using AiFashionStudio.Platform.Infrastructure.Persistence;
using AiFashionStudio.Platform.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AiFashionStudio.Platform.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("Default")));

        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));
        services.Configure<SmtpSettings>(configuration.GetSection(SmtpSettings.SectionName));

        // Entity chưa có repository riêng vẫn inject được IBaseRepository<TEntity>
        services.AddScoped(typeof(IBaseRepository<>), typeof(BaseRepository<>));

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<IPasswordResetByOtpRepository, PasswordResetByOtpRepository>();

        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        services.AddSingleton<ITokenService, TokenService>();
        services.AddSingleton<IOtpGeneratorService, OtpGeneratorService>();
        services.AddScoped<IEmailSenderService, EmailSenderService>();

        return services;
    }
}
