using AiFashionStudio.Platform.Application.Common.Interfaces.IRepositories;
using AiFashionStudio.Platform.Application.Common.Interfaces.IServices;
using AiFashionStudio.Platform.Infrastructure.Identity;
using AiFashionStudio.Platform.Infrastructure.Integration;
using AiFashionStudio.Platform.Infrastructure.Messaging;
using AiFashionStudio.Platform.Infrastructure.Payment;
using AiFashionStudio.Platform.Infrastructure.Pdf;
using AiFashionStudio.Platform.Infrastructure.Persistence;
using AiFashionStudio.Platform.Infrastructure.Persistence.Repositories;
using AiFashionStudio.Platform.Infrastructure.Storage;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AiFashionStudio.Platform.Infrastructure;

public static class DependencyInjection
{
    /// <summary>
    /// Registers infrastructure services, repositories, and configuration options.
    /// </summary>
    /// <returns>The service collection after the infrastructure registrations are added.</returns>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("Default")));

        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));
        services.Configure<SmtpSettings>(configuration.GetSection(SmtpSettings.SectionName));
        services.Configure<PayOsSettings>(configuration.GetSection(PayOsSettings.SectionName));
        services.Configure<MinioSettings>(configuration.GetSection(MinioSettings.SectionName));
        services.Configure<JavaCoreApiSettings>(configuration.GetSection(JavaCoreApiSettings.SectionName));
        services.Configure<KafkaSettings>(configuration.GetSection(KafkaSettings.SectionName));

        // Staff Gateway gọi sang java-core-api qua typed HttpClient
        var javaCoreApiSettings = configuration.GetSection(JavaCoreApiSettings.SectionName).Get<JavaCoreApiSettings>()
            ?? new JavaCoreApiSettings();
        services.AddHttpClient<IJavaCoreApiClient, JavaCoreApiClient>(client =>
        {
            if (!string.IsNullOrWhiteSpace(javaCoreApiSettings.BaseUrl))
            {
                client.BaseAddress = new Uri(javaCoreApiSettings.BaseUrl.TrimEnd('/') + "/");
            }
            client.Timeout = TimeSpan.FromSeconds(javaCoreApiSettings.TimeoutSeconds);
        });

        // Entity chưa có repository riêng vẫn inject được IBaseRepository<TEntity>
        services.AddScoped(typeof(IBaseRepository<>), typeof(BaseRepository<>));

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<IPasswordResetByOtpRepository, PasswordResetByOtpRepository>();
        services.AddScoped<IPaymentOrderRepository, PaymentOrderRepository>();
        services.AddScoped<IInvoiceRepository, InvoiceRepository>();
        services.AddScoped<IAboutUsContentRepository, AboutUsContentRepository>();
        services.AddScoped<IFeedbackRepository, FeedbackRepository>();

        //Service 
        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        services.AddSingleton<ITokenService, TokenService>();
        services.AddSingleton<IOtpGeneratorService, OtpGeneratorService>();
        services.AddScoped<IEmailSenderService, EmailSenderService>();
        services.AddSingleton<IPaymentGatewayService, PayOsPaymentGatewayService>();
        services.AddSingleton<IInvoicePdfGenerator, QuestPdfInvoiceGenerator>();
        services.AddSingleton<IFileStorage, MinioFileStorage>();

        // Kafka: producer publish PaymentSucceeded/PaymentFailed, consumer nhận OrderCreated từ Java
        services.AddSingleton<IPaymentEventPublisher, KafkaPaymentEventPublisher>();
        services.AddHostedService<OrderCreatedConsumer>();

        return services;
    }
}
