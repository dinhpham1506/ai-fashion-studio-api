using AiFashionStudio.Platform.Domain.Identity.Entities;
using AiFashionStudio.Platform.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AiFashionStudio.Platform.Infrastructure.Persistence.Configurations;

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("refresh_tokens", DatabaseSchemas.Identity);
        builder.HasKey(refreshToken => refreshToken.Id);

        builder.Property(refreshToken => refreshToken.Id).HasColumnName("id");
        builder.Property(refreshToken => refreshToken.UserId).HasColumnName("user_id");
        builder.Property(refreshToken => refreshToken.TokenHash).HasColumnName("token_hash").IsRequired();
        builder.Property(refreshToken => refreshToken.ExpiresAt).HasColumnName("expires_at");
        builder.Property(refreshToken => refreshToken.RevokedAt).HasColumnName("revoked_at");
        builder.Property(refreshToken => refreshToken.CreatedAt).HasColumnName("created_at");

        builder.HasIndex(refreshToken => refreshToken.TokenHash).IsUnique();

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(refreshToken => refreshToken.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
