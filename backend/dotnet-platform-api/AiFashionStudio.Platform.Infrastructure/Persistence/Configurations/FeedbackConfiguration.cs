using AiFashionStudio.Platform.Domain.Feedback.Entities;
using AiFashionStudio.Platform.Domain.Feedback.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AiFashionStudio.Platform.Infrastructure.Persistence.Configurations;

public class FeedbackConfiguration : IEntityTypeConfiguration<Feedback>
{
    public void Configure(EntityTypeBuilder<Feedback> builder)
    {
        builder.ToTable("feedbacks");
        builder.HasKey(feedback => feedback.Id);

        builder.Property(feedback => feedback.CustomerId).HasColumnName("customer_id").IsRequired();
        builder.Property(feedback => feedback.OrderId).HasColumnName("order_id").IsRequired();
        builder.Property(feedback => feedback.ProductId).HasColumnName("product_id").IsRequired();
        builder.Property(feedback => feedback.Rating).HasColumnName("rating").IsRequired();
        builder.Property(feedback => feedback.Comment).HasColumnName("comment");
        builder.Property(feedback => feedback.ImageUrl).HasColumnName("image_url");
        builder.Property(feedback => feedback.Status)
            .HasColumnName("status")
            .HasMaxLength(30)
            .HasConversion(
                status => status.ToString().ToUpperInvariant(),
                status => Enum.Parse<FeedbackStatus>(status, true))
            .IsRequired();
        builder.Property(feedback => feedback.ReviewedBy).HasColumnName("reviewed_by");
        builder.Property(feedback => feedback.CreatedAt).HasColumnName("created_at");
        builder.Property(feedback => feedback.UpdatedAt).HasColumnName("updated_at");

        builder.HasIndex(feedback => feedback.OrderId);
        builder.HasIndex(feedback => feedback.ProductId);
        builder.HasIndex(feedback => feedback.Status);
        builder.HasIndex(feedback => new { feedback.CustomerId, feedback.OrderId, feedback.ProductId }).IsUnique();
    }
}
