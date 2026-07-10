using System.Linq.Expressions;
using AiFashionStudio.Platform.Application.Common.Interfaces.IRepositories;
using AiFashionStudio.Platform.Domain.Common;

namespace AiFashionStudio.Platform.Tests.Common;

public class InMemoryRepository<TEntity> : IBaseRepository<TEntity>
    where TEntity : BaseEntity
{
    protected readonly List<TEntity> Store = [];

    public IReadOnlyList<TEntity> Items => Store;
    public int SaveChangesCount { get; private set; }

    public Task<TEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => Task.FromResult(Store.FirstOrDefault(entity => entity.Id == id));

    public Task<List<TEntity>> GetAllAsync(CancellationToken cancellationToken = default)
        => Task.FromResult(Store.ToList());

    public Task<List<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
        => Task.FromResult(Store.Where(predicate.Compile()).ToList());

    public Task<TEntity?> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
        => Task.FromResult(Store.FirstOrDefault(predicate.Compile()));

    public Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
        => Task.FromResult(Store.Any(predicate.Compile()));

    public Task<int> CountAsync(Expression<Func<TEntity, bool>>? predicate = null, CancellationToken cancellationToken = default)
        => Task.FromResult(predicate is null ? Store.Count : Store.Count(predicate.Compile()));

    public virtual Task AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        Store.Add(entity);
        SaveChangesCount++;
        return Task.CompletedTask;
    }

    public Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
    {
        Store.AddRange(entities);
        SaveChangesCount++;
        return Task.CompletedTask;
    }

    public Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        SaveChangesCount++;
        return Task.CompletedTask;
    }

    public Task RemoveAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        Store.Remove(entity);
        SaveChangesCount++;
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        SaveChangesCount++;
        return Task.CompletedTask;
    }
}
