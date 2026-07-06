using AiFashionStudio.Platform.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AiFashionStudio.Platform.Domain.Identity.Entities
{
    /// <summary>
    /// dữ liệu lưu trữ thông tin về việc reset mật khẩu bằng OTP cho người dùng.
    /// </summary>
    public class PasswordResetByOtp : BaseEntity
    {
        public Guid UserId { get; private set; }
        public string OtpHash { get; private set; } = string.Empty;
        public string? ResetTokenHash { get; private set; }
        public int OtpAttempts { get; private set; }
        public DateTime OtpExpiry { get; private set; }
        public DateTime? ResetTokenExpiry { get; private set; }
        public DateTime? UsedAt { get; private set; }
        public DateTime? RevokedAt { get; private set; }

        private PasswordResetByOtp() { }

        // B1: Tạo một PasswordResetByOtp mới với userId, otpHash và thời gian sống của OTP
        public static PasswordResetByOtp Create(Guid userId, string otpHash, TimeSpan lifetime)
        {
            return new PasswordResetByOtp
            {
                UserId = userId,
                OtpHash = otpHash,
                OtpExpiry = DateTime.UtcNow.Add(lifetime)
            };
        }

        // Kiểm tra xem OTP có hợp lệ hay không. Hợp lệ nếu chưa bị revoke, chưa được sử dụng, chưa có reset token và chưa hết hạn
        public bool isOtpValid() => RevokedAt is null && UsedAt is null && ResetTokenHash is null && OtpExpiry > DateTime.UtcNow;

        // Kiểm tra xem OTP đã hết hạn hay chưa, tách biệt ra để dễ test hơn
        public bool isOtpExpired() => OtpExpiry <= DateTime.UtcNow;

        // Nhập sai quá 5 lần thì revoke luôn. Trả về true nếu revoke, false nếu chưa revoke
        public bool FailOtpAttempt()
        {
            OtpAttempts++;
            if (OtpAttempts >= 5)
            {
                RevokedAt = DateTime.UtcNow;
                return true;
            }
            return false;
        }


        //B2: Gắn reset token vào PasswordResetByOtp, đồng thời set thời gian sống (lifetime) của reset token
        public void AttachResetToken(string resetTokenHash, TimeSpan lifetime)
        {
            ResetTokenHash = resetTokenHash;
            ResetTokenExpiry = DateTime.UtcNow.Add(lifetime);
        }

        // Kiểm tra xem reset token có hợp lệ hay không. Hợp lệ nếu chưa bị revoke, chưa được sử dụng, đã có reset token và chưa hết hạn
        public bool isResetTokenValid() => ResetTokenHash is not null && RevokedAt is null && UsedAt is null && ResetTokenExpiry > DateTime.UtcNow;

        // Kiểm tra xem reset token đã hết hạn hay chưa, tách biệt ra để dễ test hơn
        public bool isResetTokenExpired() => ResetTokenExpiry <= DateTime.UtcNow;

        // B3: Dánh dấu PasswordResetByOtp đã được sử dụng
        public void MarkAsUsed()
        {
            UsedAt = DateTime.UtcNow;
        }

        // B4: Dánh dấu PasswordResetByOtp đã bị revoke
        public void Revoke()
        {
            RevokedAt = DateTime.UtcNow;
        }
    }
}
