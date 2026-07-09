using FluentValidation;

namespace AiFashionStudio.Platform.Application.Identities.Commands.Register;

public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator()
    {
        RuleFor(command => command.Email)
            .NotEmpty().WithErrorCode("EMAIL_REQUIRED").WithMessage("Email is required")
            .EmailAddress().WithErrorCode("EMAIL_INVALID").WithMessage("Email is invalid");

        RuleFor(command => command.Password)
            .NotEmpty().WithErrorCode("PASSWORD_WEAK").WithMessage("Password is required")
            .MinimumLength(8).WithErrorCode("PASSWORD_WEAK").WithMessage("Password must be at least 8 characters");

        RuleFor(command => command.FullName)
            .NotEmpty().WithErrorCode("FULL_NAME_REQUIRED").WithMessage("Full name is required");
    }
}
