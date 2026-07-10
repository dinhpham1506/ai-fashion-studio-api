using AiFashionStudio.Platform.Application.Common.Dtos;
using AiFashionStudio.Platform.Domain.Feedback.Entities;
using AiFashionStudio.Platform.Domain.Feedback.Enums;

namespace AiFashionStudio.Platform.Application.Common.Interfaces.IRepositories;

public interface IFeedbackRepository : IBaseRepository<Feedback>
{
    Task<bool> ExistsByCustomerOrderProductAsync(Guid customerId, Guid orderId, Guid productId, CancellationToken cancellationToken = default);
    Task<FeedbackOrderEligibility?> GetOrderEligibilityAsync(Guid customerId, Guid orderId, Guid productId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<FeedbackListItemProjection>> GetFeedbacksAsync(
        FeedbackStatus? status,
        Guid? productId,
        bool onlyPublic,
        int skip,
        int take,
        CancellationToken cancellationToken = default);
    Task<int> CountFeedbacksAsync(
        FeedbackStatus? status,
        Guid? productId,
        bool onlyPublic,
        CancellationToken cancellationToken = default);
}
