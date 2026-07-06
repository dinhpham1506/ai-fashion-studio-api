using AiFashionStudio.Platform.Domain.Identity.Entities;

namespace AiFashionStudio.Platform.Application.Common.Interfaces.IRepositories;

public interface IPasswordResetByOtpRepository : IBaseRepository<PasswordResetByOtp>
{
    // lấy PasswordResetByOtp mới nhất của một user theo UserId, nếu không có thì trả về null
    Task<PasswordResetByOtp?> GetLatestByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    // lấy PasswordResetByOtp theo resetTokenHash, nếu không có thì trả về null
    Task<PasswordResetByOtp?> GetByResetTokenHashAsync(string resetTokenHash, CancellationToken cancellationToken = default);

    // thu hồi tất cả PasswordResetByOtp đang active của một user theo UserId
    Task RevokeAllActiveByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
}
