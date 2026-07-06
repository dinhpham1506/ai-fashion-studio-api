using AiFashionStudio.Platform.Domain.Identity.Entities;
using AiFashionStudio.Platform.Domain.Identity.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AiFashionStudio.Platform.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");
        builder.HasKey(user => user.Id);

        builder.Property(user => user.Email).HasColumnName("email").HasMaxLength(255).IsRequired();
        builder.HasIndex(user => user.Email).IsUnique();

        builder.Property(user => user.PasswordHash).HasColumnName("password_hash").IsRequired();
        builder.Property(user => user.FullName).HasColumnName("full_name").HasMaxLength(255).IsRequired();
        builder.Property(user => user.Phone).HasColumnName("phone").HasMaxLength(20);
        builder.Property(user => user.AvatarUrl).HasColumnName("avatar_url");
        builder.Property(user => user.Status)
            .HasColumnName("status")
            .HasMaxLength(30)
            .HasConversion(
                status => status.ToString().ToUpperInvariant(),
                status => Enum.Parse<UserStatus>(status, true))
            .IsRequired();
        builder.Property(user => user.CreatedAt).HasColumnName("created_at");
        builder.Property(user => user.UpdatedAt).HasColumnName("updated_at");

        builder.HasMany(user => user.UserRoles)
            .WithOne()
            .HasForeignKey(userRole => userRole.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(user => user.UserRoles)
            .HasField("_userRoles")
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
