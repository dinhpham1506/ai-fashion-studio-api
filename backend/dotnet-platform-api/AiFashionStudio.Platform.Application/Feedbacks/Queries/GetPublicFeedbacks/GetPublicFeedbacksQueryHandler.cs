using AiFashionStudio.Platform.Application.Common.Dtos;
using AiFashionStudio.Platform.Application.Common.Interfaces.IRepositories;
using MediatR;

namespace AiFashionStudio.Platform.Application.Feedbacks.Queries.GetPublicFeedbacks;

public class GetPublicFeedbacksQueryHandler : IRequestHandler<GetPublicFeedbacksQuery, PagedResult<FeedbackListItemResponse>>
{
    private readonly IFeedbackRepository _feedbackRepository;

    public GetPublicFeedbacksQueryHandler(IFeedbackRepository feedbackRepository)
    {
        _feedbackRepository = feedbackRepository;
    }

    public async Task<PagedResult<FeedbackListItemResponse>> Handle(GetPublicFeedbacksQuery query, CancellationToken cancellationToken)
    {
        var skip = (query.Page - 1) * query.PageSize;

        var items = await _feedbackRepository.GetFeedbacksAsync(
            status: null,
            productId: query.ProductId,
            onlyPublic: true,
            skip,
            query.PageSize,
            cancellationToken);

        var totalCount = await _feedbackRepository.CountFeedbacksAsync(
            status: null,
            productId: query.ProductId,
            onlyPublic: true,
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
