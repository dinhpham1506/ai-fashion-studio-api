using AiFashionStudio.Platform.Application.AiChat;
using AiFashionStudio.Platform.Application.Common.Exceptions;
using AiFashionStudio.Platform.Application.Common.Interfaces.IRepositories;
using AiFashionStudio.Platform.Domain.AiChat.Entities;
using MediatR;

namespace AiFashionStudio.Platform.Application.AiChat.Commands.ResolveAiChatConversation;

public class ResolveAiChatConversationCommandHandler : IRequestHandler<ResolveAiChatConversationCommand>
{
    private readonly IBaseRepository<AiChatConversation> _conversationRepository;

    public ResolveAiChatConversationCommandHandler(IBaseRepository<AiChatConversation> conversationRepository)
    {
        _conversationRepository = conversationRepository;
    }

    public async Task Handle(ResolveAiChatConversationCommand request, CancellationToken cancellationToken)
    {
        var conversation = await _conversationRepository.GetByIdAsync(request.ConversationId, cancellationToken)
            ?? throw new NotFoundException("AI_CHAT_CONVERSATION_NOT_FOUND", "AI chat conversation not found");

        AiChatConversationAccess.EnsureCanAccess(conversation, request.UserId);

        conversation.Resolve();
        await _conversationRepository.UpdateAsync(conversation, cancellationToken);
    }
}
