using FluentValidation;

namespace AiFashionStudio.Platform.Application.AiChat.Commands.SendAiChatMessage;

public class SendAiChatMessageCommandValidator : AbstractValidator<SendAiChatMessageCommand>
{
    public SendAiChatMessageCommandValidator()
    {
        RuleFor(command => command.ConversationId).NotEmpty();
        RuleFor(command => command.Message).NotEmpty().MaximumLength(2000);
    }
}

