using FluentValidation;

namespace AiFashionStudio.Platform.Application.Payments.Commands.CreatePayment;

public class CreatePaymentLinkCommandValidator : AbstractValidator<CreatePaymentCommand>
{
    /// <summary>
    /// Validates a payment command.
    /// </summary>
    public CreatePaymentLinkCommandValidator()
    {
        RuleFor(command => command.Amount)
            .GreaterThan(0).WithErrorCode("AMOUNT_INVALID").WithMessage("Amount must be greater than zero");

        RuleFor(command => command.Description)
            .NotEmpty().WithErrorCode("DESCRIPTION_REQUIRED").WithMessage("Description is required")
            .MaximumLength(25).WithErrorCode("DESCRIPTION_TOO_LONG").WithMessage("Description must be 25 characters or fewer");
    }
}