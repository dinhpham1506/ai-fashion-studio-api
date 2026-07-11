using FluentValidation;

namespace AiFashionStudio.Platform.Application.Staff.Queries.GetStaffOrders;

public class GetStaffOrdersQueryValidator : AbstractValidator<GetStaffOrdersQuery>
{
    // Các trạng thái Staff được phép lọc theo order lifecycle bên Java
    private static readonly string[] AllowedStatuses =
    {
        "PAID", "IN_PRODUCTION", "SHIPPING", "COMPLETED", "CANCELLED"
    };

    public GetStaffOrdersQueryValidator()
    {
        RuleFor(query => query.Page)
            .GreaterThanOrEqualTo(1)
            .WithErrorCode("INVALID_PAGE")
            .WithMessage("Page must be greater than or equal to 1");

        RuleFor(query => query.PageSize)
            .InclusiveBetween(1, 100)
            .WithErrorCode("INVALID_PAGE_SIZE")
            .WithMessage("Page size must be between 1 and 100");

        RuleFor(query => query.Status)
            .Must(status => status is null || AllowedStatuses.Contains(status.ToUpperInvariant()))
            .WithErrorCode("INVALID_ORDER_STATUS")
            .WithMessage($"Status must be one of: {string.Join(", ", AllowedStatuses)}");
    }
}
