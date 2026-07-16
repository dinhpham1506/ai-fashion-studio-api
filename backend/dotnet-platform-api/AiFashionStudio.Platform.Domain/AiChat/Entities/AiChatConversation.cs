using AiFashionStudio.Platform.Domain.AiChat.Enums;
using AiFashionStudio.Platform.Domain.Common;

namespace AiFashionStudio.Platform.Domain.AiChat.Entities;

public class AiChatConversation : UpdatableEntity
{
    public Guid? UserId { get; private set; }
    public string? UserRole { get; private set; }
    public string Channel { get; private set; } = "WEB";
    public string? PageType { get; private set; }
    public AiChatConversationStatus Status { get; private set; } = AiChatConversationStatus.Active;
    public Guid? RelatedProductId { get; private set; }
    public Guid? RelatedOrderId { get; private set; }
    public DateTime? ResolvedAt { get; private set; }

    private AiChatConversation()
    {
    }

    public static AiChatConversation Start(
        Guid? userId,
        string? userRole,
        string? channel,
        string? pageType,
        Guid? relatedProductId,
        Guid? relatedOrderId)
    {
        return new AiChatConversation
        {
            UserId = userId,
            UserRole = string.IsNullOrWhiteSpace(userRole) ? null : userRole.Trim(),
            Channel = string.IsNullOrWhiteSpace(channel) ? "WEB" : channel.Trim().ToUpperInvariant(),
            PageType = NormalizePageType(pageType),
            RelatedProductId = relatedProductId,
            RelatedOrderId = relatedOrderId
        };
    }

    public void TouchContext(string? pageType, Guid? relatedProductId, Guid? relatedOrderId)
    {
        PageType = NormalizePageType(pageType) ?? PageType;
        RelatedProductId = relatedProductId ?? RelatedProductId;
        RelatedOrderId = relatedOrderId ?? RelatedOrderId;
        Update();
    }

    public void Resolve()
    {
        Status = AiChatConversationStatus.Resolved;
        ResolvedAt = DateTime.UtcNow;
        Update();
    }

    private static string? NormalizePageType(string? pageType)
        => string.IsNullOrWhiteSpace(pageType) ? null : pageType.Trim().ToUpperInvariant();
}

