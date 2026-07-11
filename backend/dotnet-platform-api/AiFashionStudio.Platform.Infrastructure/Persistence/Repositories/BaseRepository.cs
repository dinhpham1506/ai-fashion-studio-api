using System.Linq.Expressions;
using AiFashionStudio.Platform.Application.Common.Interfaces.IRepositories;
using AiFashionStudio.Platform.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace AiFashionStudio.Platform.Infrastructure.Persistence.Repositories;

/// <summary>
/// Implementation EF Core cho <see cref="IBaseRepository{TEntity}"/> — nơi duy nhất
/// chứa code CRUD lặp lại, các repository con không phải viết lại nữa.
///
/// Cách dùng: repository của từng entity kế thừa class này + interface riêng của nó:
/// <code>
/// public class UserRepository : BaseRepository&lt;User&gt;, IUserRepository
/// {
///     public UserRepository(AppDbContext dbContext) : base(dbContext) { }
///
///     // chỉ viết các query đặc thù, dùng DbSet (protected) có sẵn từ base
///     public Task&lt;User?&gt; GetByEmailAsync(...) => DbSet.FirstOrDefaultAsync(...);
/// }
/// </code>
///
/// Mọi method đều là virtual — repository con có thể override nếu cần hành vi khác
/// (ví dụ GetByIdAsync cần Include navigation property).
/// </summary>
public class BaseRepository<TEntity> : IBaseRepository<TEntity> where TEntity : BaseEntity
{
    // protected (không phải private) để repository con dùng được khi viết query đặc thù
    protected readonly AppDbContext DbContext;

    // DbSet tương ứng với bảng của TEntity — ví dụ BaseRepository<User> thì DbSet chính là dbContext.Users
    protected readonly DbSet<TEntity> DbSet;

    public BaseRepository(AppDbContext dbContext)
    {
        DbContext = dbContext;
        // Set<TEntity>() là cách generic để lấy DbSet mà không cần biết trước tên property trong AppDbContext
        DbSet = dbContext.Set<TEntity>();
    }

    // ========== READ ==========
    // Tất cả method đều virtual: nghĩa là repository con ĐƯỢC PHÉP override để đổi hành vi.
    // Không override thì dùng nguyên bản của base — không bắt buộc phải làm gì cả.

    // virtual vì: đây là method hay bị override nhất — bản mặc định KHÔNG Include navigation
    // property. Ví dụ InvoiceRepository cần kèm Items thì override:
    //   public override Task<Invoice?> GetByIdAsync(Guid id, CancellationToken ct = default)
    //       => DbSet.Include(i => i.Items).FirstOrDefaultAsync(i => i.Id == id, ct);
    public virtual Task<TEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => DbSet.FirstOrDefaultAsync(entity => entity.Id == id, cancellationToken);

    // virtual vì: repository con có thể muốn thêm OrderBy mặc định (vd Roles sort theo Code)
    // hoặc Include kèm navigation property khi lấy cả bảng
    public virtual Task<List<TEntity>> GetAllAsync(CancellationToken cancellationToken = default)
        => DbSet.ToListAsync(cancellationToken);

    // virtual vì: có thể muốn mọi query theo điều kiện đều tự kèm Include/OrderBy,
    // override một chỗ này thay vì sửa từng nơi gọi
    public virtual Task<List<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
        => DbSet.Where(predicate).ToListAsync(cancellationToken);

    // virtual vì: giống FindAsync — muốn kèm Include thì override; ít khi cần
    public virtual Task<TEntity?> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
        => DbSet.FirstOrDefaultAsync(predicate, cancellationToken);

    // virtual chủ yếu để nhất quán — check tồn tại (SQL EXISTS) hầu như không cần override
    public virtual Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
        => DbSet.AnyAsync(predicate, cancellationToken);

    // virtual chủ yếu để nhất quán — đếm hầu như không cần override.
    // Có thể override nếu entity có soft-delete và muốn Count mặc định bỏ qua record đã xóa
    public virtual Task<int> CountAsync(Expression<Func<TEntity, bool>>? predicate = null, CancellationToken cancellationToken = default)
        => predicate is null
            ? DbSet.CountAsync(cancellationToken)
            : DbSet.CountAsync(predicate, cancellationToken);

    // ========== WRITE ==========

    // virtual vì: repository con có thể cần làm thêm việc quanh chuyện thêm mới —
    // ví dụ set giá trị mặc định, ghi log audit... — bằng cách override rồi gọi base.AddAsync(...)
    public virtual async Task AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        await DbSet.AddAsync(entity, cancellationToken);
        await DbContext.SaveChangesAsync(cancellationToken);
    }

    // virtual vì: lý do giống AddAsync — override được khi cần xử lý thêm trước/sau khi thêm hàng loạt
    public virtual async Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
    {
        await DbSet.AddRangeAsync(entities, cancellationToken);
        await DbContext.SaveChangesAsync(cancellationToken);
    }

    // virtual vì: entity có UpdatedAt (kế thừa UpdatableEntity) có thể muốn override
    // để tự gọi entity.Update() trước khi lưu, khỏi quên ở từng handler
    public virtual Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        // Entity vừa query ra từ repository thì EF đã track sẵn — SaveChanges là đủ,
        // EF tự phát hiện property nào đổi. Chỉ khi entity detached (tạo tay / đến từ
        // ngoài DbContext) mới cần Update() để attach và mark Modified.
        if (DbContext.Entry(entity).State == EntityState.Detached)
        {
            DbSet.Update(entity);
        }

        return DbContext.SaveChangesAsync(cancellationToken);
    }

    // virtual vì: đây là hard delete (xóa hẳn khỏi DB) — entity nào cần soft delete
    // (chỉ set RevokedAt/DeletedAt thay vì xóa) thì override để đổi hành vi
    public virtual Task RemoveAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        DbSet.Remove(entity);
        return DbContext.SaveChangesAsync(cancellationToken);
    }

    // virtual chủ yếu để nhất quán — chỉ là gọi thẳng SaveChanges của DbContext,
    // gần như không có lý do để override
    public virtual Task SaveChangesAsync(CancellationToken cancellationToken = default)
        => DbContext.SaveChangesAsync(cancellationToken);
}
