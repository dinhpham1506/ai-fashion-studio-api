using FluentValidation;

namespace AiFashionStudio.Platform.Application.Feedbacks.Commands.ModerateFeedback;

public class ModerateFeedbackCommandValidator : AbstractValidator<ModerateFeedbackCommand>
{
    private static readonly string[] AllowedStatuses = ["APPROVED", "HIDDEN", "REJECTED"];

    public ModerateFeedbackCommandValidator()
    {
        RuleFor(command => command.FeedbackId).NotEmpty();
        RuleFor(command => command.ReviewedBy).NotEmpty();
        RuleFor(command => command.Status)
            .NotEmpty()
            .Must(status => AllowedStatuses.Contains(status.Trim().ToUpperInvariant()))
            .WithMessage("Moderation status must be APPROVED, HIDDEN, or REJECTED.");
    }
}
