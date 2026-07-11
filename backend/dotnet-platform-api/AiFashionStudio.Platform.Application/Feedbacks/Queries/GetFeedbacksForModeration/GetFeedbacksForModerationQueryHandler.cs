using AiFashionStudio.Platform.Application.Common.Dtos;
using AiFashionStudio.Platform.Application.Common.Interfaces.IRepositories;
using AiFashionStudio.Platform.Domain.Feedback.Enums;
using MediatR;

namespace AiFashionStudio.Platform.Application.Feedbacks.Queries.GetFeedbacksForModeration;

public class GetFeedbacksForModerationQueryHandler : IRequestHandler<GetFeedbacksForModerationQuery, PagedResult<FeedbackListItemResponse>>
{
    private readonly IFeedbackRepository _feedbackRepository;

    public GetFeedbacksForModerationQueryHandler(IFeedbackRepository feedbackRepository)
    {
        _feedbackRepository = feedbackRepository;
    }

    public async Task<PagedResult<FeedbackListItemResponse>> Handle(GetFeedbacksForModerationQuery query, CancellationToken cancellationToken)
    {
        FeedbackStatus? status = query.Status is null
            ? null
            : Enum.Parse<FeedbackStatus>(query.Status, ignoreCase: true);

        var skip = (query.Page - 1) * query.PageSize;
        var items = await _feedbackRepository.GetFeedbacksAsync(
            status,
            query.ProductId,
            onlyPublic: false,
            skip,
            query.PageSize,
            cancellationToken);

        var totalCount = await _feedbackRepository.CountFeedbacksAsync(
            status,
            query.ProductId,
            onlyPublic: false,
            cancellationToken);

        return new PagedResult<FeedbackListItemResponse>(
            items.Select(Map).ToArray(),
            query.Page,
            query.PageSize,
            totalCount);
    }

    private static FeedbackListItemResponse Map(FeedbackListItemProjection item)
        => new(
            item.Id,
            item.CustomerId,
            item.CustomerName,
            item.CustomerAvatarUrl,
            item.OrderId,
            item.ProductId,
            item.Rating,
            item.Comment,
            item.ImageUrl,
            item.Status.ToString().ToUpperInvariant(),
            item.ReviewedBy,
            item.CreatedAt,
            item.UpdatedAt);
}
