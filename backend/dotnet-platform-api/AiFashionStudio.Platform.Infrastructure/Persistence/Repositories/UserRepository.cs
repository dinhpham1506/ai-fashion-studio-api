using AiFashionStudio.Platform.Application.Common.Interfaces.IRepositories;
using AiFashionStudio.Platform.Domain.Identity.Entities;
using Microsoft.EntityFrameworkCore;

namespace AiFashionStudio.Platform.Infrastructure.Persistence.Repositories;

// Kế thừa BaseRepository<User> → có sẵn GetByIdAsync, AddAsync, UpdateAsync, SaveChangesAsync...
// Ở đây chỉ viết các query đặc thù của User, query qua _context.Users
public class UserRepository : BaseRepository<User>, IUserRepository
{
    // cùng 1 instance với DbContext của base — khai báo riêng để query
    // qua _context.Users cho dễ đọc
    private readonly AppDbContext _context;

    public UserRepository(AppDbContext dbContext)
        : base(dbContext)
    {
        _context = dbContext;
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        // Bước 1: khai báo query kèm Include — bảo EF JOIN sang user_roles rồi roles
        // để navigation property UserRoles/Role có dữ liệu (login/token cần đọc role)
        var usersWithRoles = _context.Users
            .Include(user => user.UserRoles)
            .ThenInclude(userRole => userRole.Role);

        // Bước 2: tìm theo email — đến đây EF mới thật sự chạy SQL, không có thì trả về null
        var foundUser = await usersWithRoles.FirstOrDefaultAsync(
            user => user.Email == email,
            cancellationToken);

        return foundUser;
    }

    public async Task<User?> GetByIdWithRolesAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // Giống GetByEmailAsync nhưng tìm theo Id
        var usersWithRoles = _context.Users
            .Include(user => user.UserRoles)
            .ThenInclude(userRole => userRole.Role);

        var foundUser = await usersWithRoles.FirstOrDefaultAsync(
            user => user.Id == id,
            cancellationToken);

        return foundUser;
    }

    public async Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        // AnyAsync sinh SQL EXISTS — chỉ check có/không, không load dữ liệu về
        var exists = await _context.Users.AnyAsync(
            user => user.Email == email,
            cancellationToken);

        return exists;
    }
}
