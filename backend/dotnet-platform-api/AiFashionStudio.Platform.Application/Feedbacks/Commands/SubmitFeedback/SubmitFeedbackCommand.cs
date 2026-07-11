using AiFashionStudio.Platform.Application.Common.Dtos;
using MediatR;

namespace AiFashionStudio.Platform.Application.Feedbacks.Commands.SubmitFeedback;

public record SubmitFeedbackCommand(
    Guid CustomerId,
    Guid OrderId,
    Guid ProductId,
    int Rating,
    string? Comment,
    byte[]? ImageContent,
    string? ImageContentType,
    string? ImageFileName) : IRequest<FeedbackResponse>;
