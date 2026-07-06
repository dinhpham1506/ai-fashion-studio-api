using AiFashionStudio.Platform.Application.Common.Interfaces.IRepositories;
using AiFashionStudio.Platform.Domain.Identity.Entities;
using Microsoft.EntityFrameworkCore;

namespace AiFashionStudio.Platform.Infrastructure.Persistence.Repositories;

// Kế thừa BaseRepository<RefreshToken> → AddAsync/UpdateAsync có sẵn
// (handler revoke token bằng: token.Revoke() rồi UpdateAsync(token))
public class RefreshTokenRepository : BaseRepository<RefreshToken>, IRefreshTokenRepository
{
    // cùng 1 instance với DbContext của base — khai báo riêng để query
    // qua _context.RefreshTokens cho dễ đọc
    private readonly AppDbContext _context;

    public RefreshTokenRepository(AppDbContext dbContext)
        : base(dbContext)
    {
        _context = dbContext;
    }

    public async Task<RefreshToken?> GetByTokenHashAsync(string tokenHash, CancellationToken cancellationToken = default)
    {
        // Tìm đúng 1 token theo hash (token thô không bao giờ lưu DB), không có thì trả về null
        var foundToken = await _context.RefreshTokens.FirstOrDefaultAsync(
            refreshToken => refreshToken.TokenHash == tokenHash,
            cancellationToken);

        return foundToken;
    }

    public async Task RevokeAllByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        // Bước 1: lấy hết refresh token còn hiệu lực của user về bộ nhớ (EF đang track chúng)
        var activeTokens = await _context.RefreshTokens
            .Where(token => token.UserId == userId && token.RevokedAt == null)
            .ToListAsync(cancellationToken);

        // Bước 2: đánh dấu từng token là đã thu hồi (domain method set RevokedAt)
        foreach (var token in activeTokens)
        {
            token.Revoke();
        }

        // Bước 3: lưu tất cả thay đổi trong 1 lần SaveChanges
        await _context.SaveChangesAsync(cancellationToken);
    }
}
