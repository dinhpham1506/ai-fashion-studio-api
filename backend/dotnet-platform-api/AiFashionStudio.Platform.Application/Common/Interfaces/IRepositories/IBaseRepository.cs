using System.Linq.Expressions;
using AiFashionStudio.Platform.Domain.Common;

namespace AiFashionStudio.Platform.Application.Common.Interfaces.IRepositories;

/// <summary>
/// Contract chung cho mọi repository — gom các tác vụ CRUD lặp đi lặp lại về một chỗ.
///
/// Khi đó IUserRepository tự động có sẵn GetByIdAsync, AddAsync, UpdateAsync, ...
/// mà không phải khai báo lại.
///
/// TEntity bị ràng buộc phải kế thừa BaseEntity để chắc chắn có cột Id (Guid).
/// </summary>
public interface IBaseRepository<TEntity> where TEntity : BaseEntity
{

    // Lấy entity theo khóa chính Id, không có thì trả về null
    Task<TEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    // Lấy toàn bộ entity trong bảng (cẩn thận với bảng lớn — chỉ dùng cho bảng nhỏ như Roles)
    Task<List<TEntity>> GetAllAsync(CancellationToken cancellationToken = default);

    // Lấy danh sách entity thỏa điều kiện, ví dụ: FindAsync(token => token.UserId == userId)
    Task<List<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);

    // Lấy entity ĐẦU TIÊN thỏa điều kiện, không có thì trả về null
    Task<TEntity?> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);

    // Kiểm tra có tồn tại entity nào thỏa điều kiện không (dịch ra SQL EXISTS — nhanh hơn Count > 0)
    Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);

    // Đếm số entity; không truyền predicate thì đếm cả bảng
    Task<int> CountAsync(Expression<Func<TEntity, bool>>? predicate = null, CancellationToken cancellationToken = default);

    // Thêm 1 entity mới và lưu xuống DB
    Task AddAsync(TEntity entity, CancellationToken cancellationToken = default);

    // Thêm nhiều entity một lượt và lưu xuống DB (1 round-trip duy nhất)
    Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);

    // Lưu thay đổi của 1 entity (thường entity đã được track sẵn sau khi query ra rồi gọi method domain như user.ChangePassword(...))
    Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);

    // Xóa hẳn entity khỏi DB (hard delete)
    Task RemoveAsync(TEntity entity, CancellationToken cancellationToken = default);

    // Lưu mọi thay đổi đang pending của các entity được track — dùng khi sửa nhiều entity
    // rồi muốn lưu 1 lần (ví dụ revoke nhiều token), thay vì gọi UpdateAsync từng cái
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
