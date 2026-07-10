using FluentValidation;

namespace AiFashionStudio.Platform.Application.Feedbacks.Queries.GetPublicFeedbacks;

public class GetPublicFeedbacksQueryValidator : AbstractValidator<GetPublicFeedbacksQuery>
{
    public GetPublicFeedbacksQueryValidator()
    {
        RuleFor(query => query.Page).GreaterThan(0);
        RuleFor(query => query.PageSize).InclusiveBetween(1, 100);
    }
}
