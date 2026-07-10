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
        builder.HasKey(invoiceItem => invoiceItem.Id);

        builder.Property(invoiceItem => invoiceItem.ProductNameSnapshot).HasColumnName("product_name_snapshot").IsRequired();
        builder.Property(invoiceItem => invoiceItem.VariantSnapshot).HasColumnName("variant_snapshot");
        builder.Property(invoiceItem => invoiceItem.Quantity).HasColumnName("quantity");
        builder.Property(invoiceItem => invoiceItem.UnitPrice).HasColumnName("unit_price");
        builder.Property(invoiceItem => invoiceItem.CreatedAt).HasColumnName("created_at");
        builder.Property<Guid>("InvoiceId").HasColumnName("invoice_id");
    }
}
