using AiFashionStudio.Platform.Application.Common.Exceptions;
using AiFashionStudio.Platform.Domain.AiChat.Entities;

namespace AiFashionStudio.Platform.Application.AiChat;

public static class AiChatConversationAccess
{
    public static void EnsureCanAccess(AiChatConversation conversation, Guid? requesterId)
    {
        if (!conversation.UserId.HasValue)
        {
            return;
        }

        if (conversation.UserId != requesterId)
        {
            throw new ForbiddenException("AI_CHAT_CONVERSATION_FORBIDDEN", "You cannot access this conversation");
        }
    }
}
