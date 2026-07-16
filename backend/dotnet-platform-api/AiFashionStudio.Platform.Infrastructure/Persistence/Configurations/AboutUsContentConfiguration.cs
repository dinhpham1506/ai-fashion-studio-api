using AiFashionStudio.Platform.Domain.Content.Entities;
using AiFashionStudio.Platform.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AiFashionStudio.Platform.Infrastructure.Persistence.Configurations;

public class AboutUsContentConfiguration : IEntityTypeConfiguration<AboutUsContent>
{
    /// <summary>
    /// Configures the Entity Framework Core mapping for <see cref="AboutUsContent"/>.
    /// </summary>
    /// <param name="builder">The entity type builder used to configure the mapping.</param>
    public void Configure(EntityTypeBuilder<AboutUsContent> builder)
    {
        builder.ToTable("about_us_contents", DatabaseSchemas.Content);
        builder.HasKey(section => section.Id);

        builder.Property(section => section.Id).HasColumnName("id");
        builder.Property(section => section.SectionKey).HasColumnName("section_key").HasMaxLength(100).IsRequired();
        builder.Property(section => section.Title).HasColumnName("title").HasMaxLength(255).IsRequired();
        builder.Property(section => section.Content).HasColumnName("content").IsRequired();
        builder.Property(section => section.ImageUrl).HasColumnName("image_url");
        builder.Property(section => section.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(30);
        builder.Property(section => section.UpdatedBy).HasColumnName("updated_by");
        builder.Property(section => section.CreatedAt).HasColumnName("created_at");
        builder.Property(section => section.UpdatedAt).HasColumnName("updated_at");

        builder.HasIndex(section => section.SectionKey).IsUnique();
    }
}
