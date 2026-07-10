using AiFashionStudio.Platform.Domain.Identity.Entities;
using AiFashionStudio.Platform.Domain.Payment;
using AiFashionStudio.Platform.Domain.Payment.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AiFashionStudio.Platform.Infrastructure.Persistence.Configurations;

public class PaymentOrderConfiguration : IEntityTypeConfiguration<PaymentOrder>
{
    /// <summary>
    /// Configures the PaymentOrder entity mapping.
    /// </summary>
    /// <param name="builder">The entity type builder used to configure the PaymentOrder model.</param>
    public void Configure(EntityTypeBuilder<PaymentOrder> builder)
    {
        builder.ToTable("payment_orders");
        builder.HasKey(order => order.Id);

        builder.Property(order => order.UserId).HasColumnName("user_id");
        builder.Property(order => order.OrderCode).HasColumnName("order_code");
        builder.Property(order => order.Amount).HasColumnName("amount");
        builder.Property(order => order.Description).HasColumnName("description").HasMaxLength(256).IsRequired();
        builder.Property(order => order.PaymentLinkId).HasColumnName("payment_link_id");
        builder.Property(order => order.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(20);
        builder.Property(order => order.GatewayReference).HasColumnName("gateway_reference");
        builder.Property(order => order.PaidAt).HasColumnName("paid_at");
        builder.Property(order => order.CancelledAt).HasColumnName("cancelled_at");
        builder.Property(order => order.CreatedAt).HasColumnName("created_at");
        builder.Property(order => order.UpdatedAt).HasColumnName("updated_at");

        builder.HasIndex(order => order.OrderCode).IsUnique();
        builder.HasIndex(order => order.UserId);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(order => order.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
