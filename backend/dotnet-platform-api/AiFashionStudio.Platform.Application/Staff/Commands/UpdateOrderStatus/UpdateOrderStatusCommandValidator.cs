using FluentValidation;

namespace AiFashionStudio.Platform.Application.Staff.Commands.UpdateOrderStatus;

public class UpdateOrderStatusCommandValidator : AbstractValidator<UpdateOrderStatusCommand>
{
    // Trạng thái đích Staff được phép yêu cầu; transition hợp lệ hay không do Java quyết định
    private static readonly string[] AllowedTargetStatuses =
    {
        "IN_PRODUCTION", "SHIPPING", "COMPLETED", "CANCELLED"
    };

    public UpdateOrderStatusCommandValidator()
    {
        RuleFor(command => command.OrderId)
            .NotEmpty()
            .WithErrorCode("ORDER_ID_REQUIRED")
            .WithMessage("Order id is required");

        RuleFor(command => command.ToStatus)
            .NotEmpty()
            .WithErrorCode("ORDER_STATUS_REQUIRED")
            .WithMessage("Target status is required")
            .Must(status => AllowedTargetStatuses.Contains(status?.ToUpperInvariant()))
            .WithErrorCode("INVALID_ORDER_STATUS")
            .WithMessage($"Status must be one of: {string.Join(", ", AllowedTargetStatuses)}");

        RuleFor(command => command.Note)
            .MaximumLength(500)
            .WithErrorCode("NOTE_TOO_LONG")
            .WithMessage("Note must not exceed 500 characters");
    }
}
