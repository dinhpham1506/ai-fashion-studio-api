using FluentValidation;

namespace AiFashionStudio.Platform.Application.Identities.Commands.Login;

public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(command => command.Email)
            .NotEmpty().WithErrorCode("EMAIL_REQUIRED").WithMessage("Email is required");

        RuleFor(command => command.Password)
            .NotEmpty().WithErrorCode("PASSWORD_REQUIRED").WithMessage("Password is required");
    }
}
