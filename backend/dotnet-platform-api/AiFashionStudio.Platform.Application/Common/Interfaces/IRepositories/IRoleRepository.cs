using AiFashionStudio.Platform.Domain.Identity.Entities;
using AiFashionStudio.Platform.Domain.Identity.Enums;

namespace AiFashionStudio.Platform.Application.Common.Interfaces.IRepositories;


public interface IRoleRepository : IBaseRepository<Role>
{
    // Lấy role theo code (kèm quyền) — dùng cho login
    Task<Role?> GetByCodeAsync(RoleName code, CancellationToken cancellationToken = default);
}
