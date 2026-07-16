using AiFashionStudio.Platform.Domain.AiChat.Enums;
using AiFashionStudio.Platform.Domain.Common;

namespace AiFashionStudio.Platform.Domain.AiChat.Entities;

public class AiChatMessage : BaseEntity
{
    public Guid ConversationId { get; private set; }
    public AiChatSenderType SenderType { get; private set; }
    public string Content { get; private set; } = default!;
    public string? Intent { get; private set; }
    public string? MetadataJson { get; private set; }

    private AiChatMessage()
    {
    }

    public static AiChatMessage Create(
        Guid conversationId,
        AiChatSenderType senderType,
        string content,
        string? intent = null,
        string? metadataJson = null)
    {
        return new AiChatMessage
        {
            ConversationId = conversationId,
            SenderType = senderType,
            Content = content.Trim(),
            Intent = string.IsNullOrWhiteSpace(intent) ? null : intent.Trim().ToUpperInvariant(),
            MetadataJson = metadataJson
        };
    }
}

