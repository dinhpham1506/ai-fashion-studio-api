using AiFashionStudio.Platform.Application.Common.Dtos;
using MediatR;

namespace AiFashionStudio.Platform.Application.AiChat.Queries.GetAiChatConversation;

public record GetAiChatConversationQuery(Guid ConversationId, Guid? UserId) : IRequest<AiChatConversationResponse>;

