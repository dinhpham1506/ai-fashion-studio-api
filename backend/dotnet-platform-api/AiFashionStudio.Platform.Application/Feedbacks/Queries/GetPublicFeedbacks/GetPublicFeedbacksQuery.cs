using AiFashionStudio.Platform.Application.Common.Dtos;
using MediatR;

namespace AiFashionStudio.Platform.Application.Feedbacks.Queries.GetPublicFeedbacks;

public record GetPublicFeedbacksQuery(Guid? ProductId, int Page, int PageSize) : IRequest<PagedResult<FeedbackListItemResponse>>;
