using FluentValidation;

namespace AiFashionStudio.Platform.Application.Users.Commands.UpdateMyProfile
{
    public class UpdateMyProfileCommandValidator : AbstractValidator<UpdateMyProfileCommand>
    {
        public UpdateMyProfileCommandValidator()
        {
            RuleFor(command => command.FullName)
                .NotEmpty().WithErrorCode("FULL_NAME_REQUIRED").WithMessage("Full name is required")
                .MaximumLength(255).WithErrorCode("FULL_NAME_TOO_LONG").WithMessage("Full name must be 255 characters or fewer");

            RuleFor(command => command.Phone)
                .MaximumLength(20).WithErrorCode("PHONE_TOO_LONG").WithMessage("Phone must be 20 characters or fewer");
        }
    }
}
