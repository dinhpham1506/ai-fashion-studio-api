using AiFashionStudio.Platform.Domain.Common;
using AiFashionStudio.Platform.Domain.Feedback.Enums;

namespace AiFashionStudio.Platform.Domain.Feedback.Entities;

public class Feedback : UpdatableEntity
{
    public Guid CustomerId { get; private set; }
    public Guid OrderId { get; private set; }
    public Guid ProductId { get; private set; }
    public int Rating { get; private set; }
    public string? Comment { get; private set; }
    public string? ImageUrl { get; private set; }
    public FeedbackStatus Status { get; private set; } = FeedbackStatus.Pending;
    public Guid? ReviewedBy { get; private set; }

    private Feedback()
    {
    }

    public static Feedback Create(Guid customerId, Guid orderId, Guid productId, int rating, string? comment, string? imageUrl)
    {
        return new Feedback
        {
            CustomerId = customerId,
            OrderId = orderId,
            ProductId = productId,
            Rating = rating,
            Comment = comment,
            ImageUrl = imageUrl,
            Status = FeedbackStatus.Pending
        };
    }

    public void Moderate(FeedbackStatus status, Guid reviewedBy)
    {
        Status = status;
        ReviewedBy = reviewedBy;
        Update();
    }
}
