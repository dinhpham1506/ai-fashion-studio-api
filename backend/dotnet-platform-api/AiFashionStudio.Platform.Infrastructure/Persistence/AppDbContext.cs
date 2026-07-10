using AiFashionStudio.Platform.Domain.Content.Entities;
using AiFashionStudio.Platform.Domain.Feedback.Entities;
using AiFashionStudio.Platform.Domain.Identity.Entities;
using AiFashionStudio.Platform.Domain.Invoice.Entities;
using AiFashionStudio.Platform.Domain.Payment.Entities;
using Microsoft.EntityFrameworkCore;

namespace AiFashionStudio.Platform.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();

    public DbSet<Role> Roles => Set<Role>();

    public DbSet<UserRole> UserRoles => Set<UserRole>();

    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    public DbSet<PasswordResetByOtp> PasswordResetByOtps => Set<PasswordResetByOtp>();

    public DbSet<PaymentOrder> PaymentOrders => Set<PaymentOrder>();

    public DbSet<Invoice> Invoices => Set<Invoice>();

    public DbSet<AboutUsContent> AboutUsContents => Set<AboutUsContent>();

    /// <summary>
    /// Applies entity configurations for the context.
    /// </summary>
    /// <param name="modelBuilder">The builder used to configure the model.</param>

    public DbSet<Feedback> Feedbacks => Set<Feedback>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
