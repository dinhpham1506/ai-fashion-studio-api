using AiFashionStudio.Platform.Domain.Common;

namespace AiFashionStudio.Platform.Domain.AiChat.Entities;

public class AiChatSupportTicket : UpdatableEntity
{
    public Guid ConversationId { get; private set; }
    public Guid? UserId { get; private set; }
    public Guid? OrderId { get; private set; }
    public string IssueType { get; private set; } = default!;
    public string Severity { get; private set; } = "NORMAL";
    public string Status { get; private set; } = "OPEN";
    public Guid? AssignedTo { get; private set; }
    public string Summary { get; private set; } = default!;

    private AiChatSupportTicket()
    {
    }

    public static AiChatSupportTicket Create(
        Guid conversationId,
        Guid? userId,
        Guid? orderId,
        string issueType,
        string summary)
    {
        return new AiChatSupportTicket
        {
            ConversationId = conversationId,
            UserId = userId,
            OrderId = orderId,
            IssueType = issueType.Trim().ToUpperInvariant(),
            Summary = summary.Trim()
        };
    }
}

