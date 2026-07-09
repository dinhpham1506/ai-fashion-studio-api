using AiFashionStudio.Platform.Domain.Invoice.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AiFashionStudio.Platform.Infrastructure.Persistence.Configurations;

public class InvoiceItemConfiguration : IEntityTypeConfiguration<InvoiceItem>
{
    /// <summary>
    /// Configures the database mapping for <see cref="InvoiceItem"/>.
    /// </summary>
    public void Configure(EntityTypeBuilder<InvoiceItem> builder)
    {
        builder.ToTable("invoice_items");
        builder.HasKey(item => item.Id);

        builder.Property(item => item.ProductNameSnapshot).HasColumnName("product_name_snapshot").IsRequired();
        builder.Property(item => item.VariantSnapshot).HasColumnName("variant_snapshot");
        builder.Property(item => item.Quantity).HasColumnName("quantity");
        builder.Property(item => item.UnitPrice).HasColumnName("unit_price");
        builder.Property(item => item.CreatedAt).HasColumnName("created_at");

        builder.Ignore(item => item.TotalPrice);
    }
}
