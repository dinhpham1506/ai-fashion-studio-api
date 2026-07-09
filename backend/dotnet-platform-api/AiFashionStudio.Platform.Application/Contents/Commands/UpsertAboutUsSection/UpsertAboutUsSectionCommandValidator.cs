using FluentValidation;
using System;

namespace AiFashionStudio.Platform.Application.Contents.Commands.UpsertAboutUsSection
{
    public class UpsertAboutUsSectionCommandValidator : AbstractValidator<UpsertAboutUsSectionCommand>
    {
        public UpsertAboutUsSectionCommandValidator()
        {
            RuleFor(command => command.SectionKey)
                .NotEmpty().WithErrorCode("SECTION_KEY_REQUIRED").WithMessage("Section key is required")
                .MaximumLength(100).WithErrorCode("SECTION_KEY_TOO_LONG").WithMessage("Section key must be 100 characters or fewer");

            RuleFor(command => command.Title)
                .NotEmpty().WithErrorCode("TITLE_REQUIRED").WithMessage("Title is required")
                .MaximumLength(255).WithErrorCode("TITLE_TOO_LONG").WithMessage("Title must be 255 characters or fewer");

            RuleFor(command => command.Content)
                .NotEmpty().WithErrorCode("CONTENT_REQUIRED").WithMessage("Content is required");

            RuleFor(command => command.Status)
                .Must(status => string.Equals(status, "DRAFT", StringComparison.OrdinalIgnoreCase)
                             || string.Equals(status, "PUBLISHED", StringComparison.OrdinalIgnoreCase))
                .WithErrorCode("INVALID_STATUS").WithMessage("Status must be DRAFT or PUBLISHED");
        }
    }
}
