using AiFashionStudio.Platform.Domain.Identity.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AiFashionStudio.Platform.Infrastructure.Persistence.Configurations;

public class UserRoleConfiguration : IEntityTypeConfiguration<UserRole>
{
    public void Configure(EntityTypeBuilder<UserRole> builder)
    {
        builder.ToTable("user_roles");
        builder.HasKey(userRole => userRole.Id);

        builder.Property(userRole => userRole.UserId).HasColumnName("user_id");
        builder.Property(userRole => userRole.RoleId).HasColumnName("role_id");
        builder.Property(userRole => userRole.CreatedAt).HasColumnName("created_at");

        builder.HasIndex(userRole => new { userRole.UserId, userRole.RoleId }).IsUnique();

        builder.HasOne(userRole => userRole.Role)
            .WithMany()
            .HasForeignKey(userRole => userRole.RoleId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
