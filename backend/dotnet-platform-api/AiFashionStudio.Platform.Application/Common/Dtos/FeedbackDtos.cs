using AiFashionStudio.Platform.Domain.Feedback.Enums;

namespace AiFashionStudio.Platform.Application.Common.Dtos;

public record FeedbackResponse(
    Guid Id,
    Guid CustomerId,
    Guid OrderId,
    Guid ProductId,
    int Rating,
    string? Comment,
    string? ImageUrl,
    string Status,
    Guid? ReviewedBy,
    DateTime CreatedAt,
    DateTime UpdatedAt);

public record FeedbackListItemResponse(
    Guid Id,
    Guid CustomerId,
    string CustomerName,
    string? CustomerAvatarUrl,
    Guid OrderId,
    Guid ProductId,
    int Rating,
    string? Comment,
    string? ImageUrl,
    string Status,
    Guid? ReviewedBy,
    DateTime CreatedAt,
    DateTime UpdatedAt);

public sealed class FeedbackOrderEligibility
{
    public bool OrderExists { get; init; }
    public bool IsCompleted { get; init; }
    public bool ProductBelongsToOrder { get; init; }
}

public sealed class FeedbackListItemProjection
{
    public Guid Id { get; init; }
    public Guid CustomerId { get; init; }
    public string CustomerName { get; init; } = string.Empty;
    public string? CustomerAvatarUrl { get; init; }
    public Guid OrderId { get; init; }
    public Guid ProductId { get; init; }
    public int Rating { get; init; }
    public string? Comment { get; init; }
    public string? ImageUrl { get; init; }
    public FeedbackStatus Status { get; init; }
    public Guid? ReviewedBy { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}
