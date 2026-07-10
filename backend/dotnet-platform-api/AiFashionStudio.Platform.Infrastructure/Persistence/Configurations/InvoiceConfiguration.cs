using AiFashionStudio.Platform.Domain.Invoice.Entities;
using AiFashionStudio.Platform.Domain.Identity.Entities;
using AiFashionStudio.Platform.Domain.Payment.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AiFashionStudio.Platform.Infrastructure.Persistence.Configurations;

public class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
{
    /// <summary>
    /// Configures the entity mapping for invoices.
    /// </summary>
    public void Configure(EntityTypeBuilder<Invoice> builder)
    {
        builder.ToTable("invoices");
        builder.HasKey(invoice => invoice.Id);

        builder.Property(invoice => invoice.OrderId).HasColumnName("order_id");
        builder.Property(invoice => invoice.PaymentId).HasColumnName("payment_id");
        builder.Property(invoice => invoice.CustomerId).HasColumnName("customer_id");
        builder.Property(invoice => invoice.InvoiceNumber).HasColumnName("invoice_number").IsRequired();
        builder.Property(invoice => invoice.TotalAmount).HasColumnName("total_amount");
        builder.Property(invoice => invoice.Currency).HasColumnName("currency").HasMaxLength(3);
        builder.Property(invoice => invoice.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(20);
        builder.Property(invoice => invoice.PdfUrl).HasColumnName("pdf_url");
        builder.Property(invoice => invoice.IssuedAt).HasColumnName("issued_at");
        builder.Property(invoice => invoice.CreatedAt).HasColumnName("created_at");

        builder.HasIndex(invoice => invoice.OrderId).IsUnique();
        builder.HasIndex(invoice => invoice.InvoiceNumber).IsUnique();

        builder.HasOne<PaymentOrder>()
            .WithMany()
            .HasForeignKey(invoice => invoice.PaymentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(invoice => invoice.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(invoice => invoice.Items)
            .WithOne()
            .HasForeignKey("InvoiceId")
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(invoice => invoice.Items)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
