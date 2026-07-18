using AiFashionStudio.Platform.Application.Common.Dtos;
using MediatR;

namespace AiFashionStudio.Platform.Application.AiChat.Commands.StartAiChatConversation;

public record StartAiChatConversationCommand(
    Guid? UserId,
    string? UserRole,
    string? Channel,
    AiChatPageContext? Page) : IRequest<AiChatResponse>;

