using AiFashionStudio.Platform.Domain.Identity.Entities;
using AiFashionStudio.Platform.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AiFashionStudio.Platform.Infrastructure.Persistence.Configurations;

public class PasswordResetOtpConfiguration : IEntityTypeConfiguration<PasswordResetByOtp>
{
    public void Configure(EntityTypeBuilder<PasswordResetByOtp> builder)
    {
        builder.ToTable("password_reset_otps", DatabaseSchemas.Identity);
        builder.HasKey(otp => otp.Id);

        builder.Property(otp => otp.Id).HasColumnName("id");
        builder.Property(otp => otp.UserId).HasColumnName("user_id");
        builder.Property(otp => otp.OtpHash).HasColumnName("otp_hash").IsRequired();
        builder.Property(otp => otp.ResetTokenHash).HasColumnName("reset_token_hash");
        builder.Property(otp => otp.OtpAttempts).HasColumnName("attempt_count");
        builder.Property(otp => otp.OtpExpiry).HasColumnName("expires_at");
        builder.Property(otp => otp.ResetTokenExpiry).HasColumnName("reset_token_expires_at");
        builder.Property(otp => otp.UsedAt).HasColumnName("used_at");
        builder.Property(otp => otp.RevokedAt).HasColumnName("revoked_at");
        builder.Property(otp => otp.CreatedAt).HasColumnName("created_at");

        builder.HasIndex(otp => otp.UserId);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(otp => otp.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
