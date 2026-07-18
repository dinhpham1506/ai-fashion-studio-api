using AiFashionStudio.Platform.Application.Common.Dtos;
using MediatR;

namespace AiFashionStudio.Platform.Application.AiChat.Commands.SendAiChatMessage;

public record SendAiChatMessageCommand(
    Guid ConversationId,
    Guid? UserId,
    string? UserRole,
    string Message,
    AiChatPageContext? Page,
    AiChatClientContext? ClientContext) : IRequest<AiChatResponse>;
