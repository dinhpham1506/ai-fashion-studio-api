using AiFashionStudio.Platform.Application.AiChat;
using AiFashionStudio.Platform.Application.Common.Dtos;
using AiFashionStudio.Platform.Application.Common.Exceptions;
using AiFashionStudio.Platform.Application.Common.Interfaces.IRepositories;
using AiFashionStudio.Platform.Domain.AiChat.Entities;
using MediatR;

namespace AiFashionStudio.Platform.Application.AiChat.Queries.GetAiChatConversation;

public class GetAiChatConversationQueryHandler : IRequestHandler<GetAiChatConversationQuery, AiChatConversationResponse>
{
    private readonly IBaseRepository<AiChatConversation> _conversationRepository;
    private readonly IBaseRepository<AiChatMessage> _messageRepository;

    public GetAiChatConversationQueryHandler(
        IBaseRepository<AiChatConversation> conversationRepository,
        IBaseRepository<AiChatMessage> messageRepository)
    {
        _conversationRepository = conversationRepository;
        _messageRepository = messageRepository;
    }

    public async Task<AiChatConversationResponse> Handle(GetAiChatConversationQuery request, CancellationToken cancellationToken)
    {
        var conversation = await _conversationRepository.GetByIdAsync(request.ConversationId, cancellationToken)
            ?? throw new NotFoundException("AI_CHAT_CONVERSATION_NOT_FOUND", "AI chat conversation not found");

        AiChatConversationAccess.EnsureCanAccess(conversation, request.UserId);

        var messages = await _messageRepository.FindAsync(
            message => message.ConversationId == conversation.Id,
            cancellationToken);

        return new AiChatConversationResponse(
            conversation.Id,
            conversation.Status.ToString().ToUpperInvariant(),
            conversation.PageType,
            conversation.RelatedProductId,
            conversation.RelatedOrderId,
            messages
                .OrderBy(message => message.CreatedAt)
                .Select(message => new AiChatMessageResponse(
                    message.Id,
                    message.SenderType.ToString().ToUpperInvariant(),
                    message.Content,
                    message.Intent,
                    message.CreatedAt))
                .ToList());
    }
}
