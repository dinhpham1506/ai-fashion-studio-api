using AiFashionStudio.Platform.Application.Common.Interfaces.IRepositories;
using AiFashionStudio.Platform.Domain.Identity.Entities;
using Microsoft.EntityFrameworkCore;

namespace AiFashionStudio.Platform.Infrastructure.Persistence.Repositories;

public class PasswordResetByOtpRepository : BaseRepository<PasswordResetByOtp>, IPasswordResetByOtpRepository
{
    // cùng 1 instance với DbContext của base — khai báo riêng để query
    // qua _context.PasswordResetOtps cho dễ đọc
    private readonly AppDbContext _context;

    public PasswordResetByOtpRepository(AppDbContext dbContext)
        : base(dbContext)
    {
        _context = dbContext;
    }

    public async Task<PasswordResetByOtp?> GetLatestByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        // Bước 1: lọc các OTP của user này, chưa bị thu hồi và chưa được dùng
        var activeOtpsQuery = _context.PasswordResetByOtps.Where(otp =>
            otp.UserId == userId &&
            otp.RevokedAt == null &&
            otp.UsedAt == null);

        // Bước 2: sắp xếp cái mới tạo nhất lên đầu
        var sortedQuery = activeOtpsQuery.OrderByDescending(otp => otp.CreatedAt);

        // Bước 3: lấy phần tử đầu tiên — đến đây EF mới thật sự chạy SQL.
        // Không tìm thấy thì trả về null (nên kiểu trả về là PasswordResetOtp?)
        var latestOtp = await sortedQuery.FirstOrDefaultAsync(cancellationToken);

        return latestOtp;
    }

    public async Task<PasswordResetByOtp?> GetByResetTokenHashAsync(string resetTokenHash, CancellationToken cancellationToken = default)
    {
        // Tìm đúng 1 record theo hash của reset token, không có thì trả về null.
        // (Biến kết quả không được trùng tên với tham số lambda `otp` — C# báo lỗi CS0136)
        var foundOtp = await _context.PasswordResetByOtps.FirstOrDefaultAsync(
            otp => otp.ResetTokenHash == resetTokenHash,
            cancellationToken);

        return foundOtp;
    }

    public async Task RevokeAllActiveByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        // Bước 1: lấy hết OTP còn hiệu lực của user về bộ nhớ (EF đang track chúng)
        var activeOtps = await _context.PasswordResetByOtps
            .Where(otp =>
                otp.UserId == userId &&
                otp.RevokedAt == null &&
                otp.UsedAt == null)
            .ToListAsync(cancellationToken);

        // Bước 2: đánh dấu từng cái là đã thu hồi (domain method set RevokedAt)
        foreach (var otp in activeOtps)
        {
            otp.Revoke();
        }

        // Bước 3: lưu — EF tự thấy các entity đã đổi và sinh UPDATE tương ứng
        await _context.SaveChangesAsync(cancellationToken);
    }
}