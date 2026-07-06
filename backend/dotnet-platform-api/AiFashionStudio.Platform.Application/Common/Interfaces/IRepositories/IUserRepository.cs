using AiFashionStudio.Platform.Domain.Identity.Entities;

namespace AiFashionStudio.Platform.Application.Common.Interfaces.IRepositories;

// Kế thừa IBaseRepository<User> → AddAsync, UpdateAsync, GetByIdAsync... có sẵn,
// chỉ khai báo thêm các query đặc thù của User
public interface IUserRepository : IBaseRepository<User>
{
    // Lấy user theo email (kèm roles) — dùng cho login
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);

    // Lấy user theo id kèm roles — khác GetByIdAsync của base (không Include roles)
    Task<User?> GetByIdWithRolesAsync(Guid id, CancellationToken cancellationToken = default);

    // Check email đã đăng ký chưa — dùng cho register
    Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default);
}
