using AiFashionStudio.Platform.Application.Common.Dtos;
using AiFashionStudio.Platform.Application.Common.Interfaces.IRepositories;
using AiFashionStudio.Platform.Domain.Feedback.Entities;
using AiFashionStudio.Platform.Domain.Feedback.Enums;
using Microsoft.EntityFrameworkCore;

namespace AiFashionStudio.Platform.Infrastructure.Persistence.Repositories;

public class FeedbackRepository : BaseRepository<Feedback>, IFeedbackRepository
{
    private readonly AppDbContext _context;

    public FeedbackRepository(AppDbContext dbContext)
        : base(dbContext)
    {
        _context = dbContext;
    }

    public Task<bool> ExistsByCustomerOrderProductAsync(Guid customerId, Guid orderId, Guid productId, CancellationToken cancellationToken = default)
        => _context.Feedbacks.AnyAsync(
            feedback => feedback.CustomerId == customerId
                && feedback.OrderId == orderId
                && feedback.ProductId == productId,
            cancellationToken);

    public async Task<FeedbackOrderEligibility?> GetOrderEligibilityAsync(Guid customerId, Guid orderId, Guid productId, CancellationToken cancellationToken = default)
    {
        var result = await _context.Database.SqlQuery<FeedbackOrderEligibility>($@"
            SELECT
                TRUE AS ""OrderExists"",
                o.order_status = 'COMPLETED' AS ""IsCompleted"",
                EXISTS (
                    SELECT 1
                    FROM order_items oi
                    WHERE oi.order_id = o.id AND oi.product_id = {productId}
                ) AS ""ProductBelongsToOrder""
            FROM orders o
            WHERE o.id = {orderId} AND o.customer_id = {customerId}")
            .FirstOrDefaultAsync(cancellationToken);

        return result;
    }

    public async Task<IReadOnlyList<FeedbackListItemProjection>> GetFeedbacksAsync(
        FeedbackStatus? status,
        Guid? productId,
        bool onlyPublic,
        int skip,
        int take,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Feedbacks
            .AsNoTracking()
            .Join(
                _context.Users.AsNoTracking(),
                feedback => feedback.CustomerId,
                user => user.Id,
                (feedback, user) => new FeedbackListItemProjection
                {
                    Id = feedback.Id,
                    CustomerId = feedback.CustomerId,
                    CustomerName = user.FullName,
                    CustomerAvatarUrl = user.AvatarUrl,
                    OrderId = feedback.OrderId,
                    ProductId = feedback.ProductId,
                    Rating = feedback.Rating,
                    Comment = feedback.Comment,
                    ImageUrl = feedback.ImageUrl,
                    Status = feedback.Status,
                    ReviewedBy = feedback.ReviewedBy,
                    CreatedAt = feedback.CreatedAt,
                    UpdatedAt = feedback.UpdatedAt
                });

        if (onlyPublic)
        {
            query = query.Where(item => item.Status == FeedbackStatus.Approved);
        }
        else if (status is not null)
        {
            query = query.Where(item => item.Status == status.Value);
        }

        if (productId is not null)
        {
            query = query.Where(item => item.ProductId == productId.Value);
        }

        return await query
            .OrderByDescending(item => item.CreatedAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> CountFeedbacksAsync(
        FeedbackStatus? status,
        Guid? productId,
        bool onlyPublic,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Feedbacks.AsNoTracking().AsQueryable();

        if (onlyPublic)
        {
            query = query.Where(feedback => feedback.Status == FeedbackStatus.Approved);
        }
        else if (status is not null)
        {
            query = query.Where(feedback => feedback.Status == status.Value);
        }

        if (productId is not null)
        {
            query = query.Where(feedback => feedback.ProductId == productId.Value);
        }

        return await query.CountAsync(cancellationToken);
    }
}
