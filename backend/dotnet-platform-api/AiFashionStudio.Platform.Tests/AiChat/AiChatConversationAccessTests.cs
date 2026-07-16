using AiFashionStudio.Platform.Application.AiChat.Queries.GetAiChatConversation;
using AiFashionStudio.Platform.Application.Common.Exceptions;
using AiFashionStudio.Platform.Domain.AiChat.Entities;
using AiFashionStudio.Platform.Tests.Common;
using Xunit;

namespace AiFashionStudio.Platform.Tests.AiChat;

public class AiChatConversationAccessTests
{
    [Fact]
    public async Task GetConversation_Should_Allow_Anonymous_Request_To_Resume_Widget_Conversation()
    {
        var ownerId = Guid.NewGuid();
        var conversation = AiChatConversation.Start(ownerId, "CUSTOMER", "WEB", "PRODUCT_DETAIL", Guid.NewGuid(), null);
        var conversations = new InMemoryRepository<AiChatConversation>();
        var messages = new InMemoryRepository<AiChatMessage>();
        await conversations.AddAsync(conversation);

        var handler = new GetAiChatConversationQueryHandler(conversations, messages);

        var result = await handler.Handle(new GetAiChatConversationQuery(conversation.Id, UserId: null), CancellationToken.None);

        Assert.Equal(conversation.Id, result.ConversationId);
    }

    [Fact]
    public async Task GetConversation_Should_Reject_Different_Authenticated_User()
    {
        var conversation = AiChatConversation.Start(Guid.NewGuid(), "CUSTOMER", "WEB", "PRODUCT_DETAIL", Guid.NewGuid(), null);
        var conversations = new InMemoryRepository<AiChatConversation>();
        var messages = new InMemoryRepository<AiChatMessage>();
        await conversations.AddAsync(conversation);

        var handler = new GetAiChatConversationQueryHandler(conversations, messages);

        var exception = await Assert.ThrowsAsync<ForbiddenException>(
            () => handler.Handle(new GetAiChatConversationQuery(conversation.Id, Guid.NewGuid()), CancellationToken.None));

        Assert.Equal("AI_CHAT_CONVERSATION_FORBIDDEN", exception.Errors.Single().Code);
    }
}
