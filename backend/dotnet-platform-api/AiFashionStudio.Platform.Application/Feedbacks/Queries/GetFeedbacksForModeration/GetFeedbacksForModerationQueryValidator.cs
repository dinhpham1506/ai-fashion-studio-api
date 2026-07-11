using FluentValidation;

namespace AiFashionStudio.Platform.Application.Feedbacks.Queries.GetFeedbacksForModeration;

public class GetFeedbacksForModerationQueryValidator : AbstractValidator<GetFeedbacksForModerationQuery>
{
    private static readonly string[] AllowedStatuses = ["PENDING", "APPROVED", "HIDDEN", "REJECTED"];

    public GetFeedbacksForModerationQueryValidator()
    {
        RuleFor(query => query.Page).GreaterThan(0);
        RuleFor(query => query.PageSize).InclusiveBetween(1, 100);
        RuleFor(query => query.Status)
            .Must(status => status is null || AllowedStatuses.Contains(status.Trim().ToUpperInvariant()))
            .WithMessage("Status must be PENDING, APPROVED, HIDDEN, or REJECTED.");
    }
}
