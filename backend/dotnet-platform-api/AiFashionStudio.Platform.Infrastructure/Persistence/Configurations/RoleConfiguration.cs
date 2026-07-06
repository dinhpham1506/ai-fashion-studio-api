using AiFashionStudio.Platform.Domain.Identity.Entities;
using AiFashionStudio.Platform.Domain.Identity.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AiFashionStudio.Platform.Infrastructure.Persistence.Configurations;

public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("roles");
        builder.HasKey(role => role.Id);

        builder.Property(role => role.Code)
            .HasColumnName("code")
            .HasMaxLength(50)
            .HasConversion(
                code => code.ToString().ToUpperInvariant(),
                code => Enum.Parse<RoleName>(code, true))
            .IsRequired();

        builder.HasIndex(role => role.Code).IsUnique();

        builder.Property(role => role.Name).HasColumnName("name").HasMaxLength(100).IsRequired();
        builder.Property(role => role.Description).HasColumnName("description");
        builder.Property(role => role.CreatedAt).HasColumnName("created_at");
    }
}
