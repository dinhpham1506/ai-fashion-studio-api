using AiFashionStudio.Platform.Application.Common.Interfaces.IRepositories;
using AiFashionStudio.Platform.Domain.Identity.Entities;
using AiFashionStudio.Platform.Domain.Identity.Enums;
using Microsoft.EntityFrameworkCore;

namespace AiFashionStudio.Platform.Infrastructure.Persistence.Repositories;

// Kế thừa BaseRepository<Role> → CRUD chung có sẵn, chỉ thêm query theo RoleName
public class RoleRepository : BaseRepository<Role>, IRoleRepository
{
    // cùng 1 instance với DbContext của base — khai báo riêng để query
    // qua _context.Roles cho dễ đọc
    private readonly AppDbContext _context;

    public RoleRepository(AppDbContext dbContext)
        : base(dbContext)
    {
        _context = dbContext;
    }

    public async Task<Role?> GetByCodeAsync(RoleName code, CancellationToken cancellationToken = default)
    {
        // Tìm đúng 1 role theo code (enum RoleName), không có thì trả về null
        var foundRole = await _context.Roles.FirstOrDefaultAsync(
            role => role.Code == code,
            cancellationToken);

        return foundRole;
    }
}
