using MediatR;

namespace AiFashionStudio.Platform.Application.AiChat.Commands.ResolveAiChatConversation;

public record ResolveAiChatConversationCommand(Guid ConversationId, Guid? UserId) : IRequest;

