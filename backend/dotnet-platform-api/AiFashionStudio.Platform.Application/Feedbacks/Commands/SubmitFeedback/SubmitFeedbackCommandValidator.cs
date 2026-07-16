using FluentValidation;

namespace AiFashionStudio.Platform.Application.Feedbacks.Commands.SubmitFeedback;

public class SubmitFeedbackCommandValidator : AbstractValidator<SubmitFeedbackCommand>
{
    private static readonly string[] AllowedImageTypes = ["image/jpeg", "image/png", "image/webp"];

    public SubmitFeedbackCommandValidator()
    {
        RuleFor(command => command.OrderId).NotEmpty();
        RuleFor(command => command.ProductId).NotEmpty();
        RuleFor(command => command.Rating).InclusiveBetween(1, 5);
        RuleFor(command => command.Comment).MaximumLength(1000);

        RuleFor(command => command.ImageContent)
            .Must(content => content is null || content.Length <= 5 * 1024 * 1024)
            .WithMessage("Feedback image must be 5MB or smaller.");

        RuleFor(command => command.ImageContentType)
            .Must(contentType => contentType is null || AllowedImageTypes.Contains(contentType, StringComparer.OrdinalIgnoreCase))
            .WithMessage("Feedback image must be a JPG, PNG, or WEBP file.");
    }
}
