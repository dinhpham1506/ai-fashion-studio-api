using FluentValidation;

namespace AiFashionStudio.Platform.Application.Identity.Commands.RefreshAccessToken;

public class RefreshAccessTokenCommandValidator : AbstractValidator<RefreshAccessTokenCommand>
{
    public RefreshAccessTokenCommandValidator()
    {
        RuleFor(command => command.RefreshToken)
            .NotEmpty().WithErrorCode("REFRESH_TOKEN_REQUIRED").WithMessage("Refresh token is required");
    }
}
