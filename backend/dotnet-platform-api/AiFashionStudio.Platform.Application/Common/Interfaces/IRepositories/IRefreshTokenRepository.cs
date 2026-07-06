using AiFashionStudio.Platform.Domain.Identity.Entities;

namespace AiFashionStudio.Platform.Application.Common.Interfaces.IRepositories;

// Kế thừa IBaseRepository<RefreshToken> → AddAsync/UpdateAsync có sẵn.
// Revoke token = gọi token.Revoke() (domain) rồi UpdateAsync(token) — không cần method riêng
public interface IRefreshTokenRepository : IBaseRepository<RefreshToken>
{
    // Tìm refresh token theo hash — token thô không bao giờ lưu DB
    Task<RefreshToken?> GetByTokenHashAsync(string tokenHash, CancellationToken cancellationToken = default);

    // Thu hồi tất cả refresh token của một user theo UserId
    Task RevokeAllByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
}
