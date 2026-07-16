using AiFashionStudio.Platform.Application.Common.Dtos;
using MediatR;

namespace AiFashionStudio.Platform.Application.Feedbacks.Queries.GetFeedbacksForModeration;

public record GetFeedbacksForModerationQuery(string? Status, Guid? ProductId, int Page, int PageSize) : IRequest<PagedResult<FeedbackListItemResponse>>;
