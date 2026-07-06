using AiFashionStudio.Platform.Domain.Common;

namespace AiFashionStudio.Platform.Domain.Identity.Entities;

/// <summary>
///  dữ liệu refresh token được lưu trữ trong cơ sở dữ liệu để xác thực người dùng mà không cần yêu cầu họ đăng nhập lại.
/// </summary>
public class RefreshToken : BaseEntity
{
    public Guid UserId { get; private set; }

    public string TokenHash { get; private set; } = string.Empty;

    public DateTime ExpiresAt { get; private set; }

    public DateTime? RevokedAt { get; private set; }

    private RefreshToken()
    {
    }

    private RefreshToken(Guid userId, string tokenHash, DateTime expiresAt)
    {
        UserId = userId;
        TokenHash = tokenHash;
        ExpiresAt = expiresAt;
    }

    public static RefreshToken Create(Guid userId, string tokenHash, DateTime expiresAt)
        => new(userId, tokenHash, expiresAt);

    public bool IsActive() => RevokedAt is null && ExpiresAt > DateTime.UtcNow;

    public void Revoke() => RevokedAt = DateTime.UtcNow;
}
