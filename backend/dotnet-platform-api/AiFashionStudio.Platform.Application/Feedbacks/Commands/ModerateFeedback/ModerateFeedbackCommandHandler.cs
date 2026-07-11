using AiFashionStudio.Platform.Application.Common.Dtos;
using AiFashionStudio.Platform.Application.Common.Exceptions;
using AiFashionStudio.Platform.Application.Common.Interfaces.IRepositories;
using AiFashionStudio.Platform.Domain.Feedback.Enums;
using MediatR;

namespace AiFashionStudio.Platform.Application.Feedbacks.Commands.ModerateFeedback;

public class ModerateFeedbackCommandHandler : IRequestHandler<ModerateFeedbackCommand, FeedbackResponse>
{
    private readonly IFeedbackRepository _feedbackRepository;

    public ModerateFeedbackCommandHandler(IFeedbackRepository feedbackRepository)
    {
        _feedbackRepository = feedbackRepository;
    }

    public async Task<FeedbackResponse> Handle(ModerateFeedbackCommand command, CancellationToken cancellationToken)
    {
        var feedback = await _feedbackRepository.GetByIdAsync(command.FeedbackId, cancellationToken)
            ?? throw new NotFoundException("FEEDBACK_NOT_FOUND", "Feedback not found.");

        var status = Enum.Parse<FeedbackStatus>(command.Status, ignoreCase: true);
        feedback.Moderate(status, command.ReviewedBy);
        await _feedbackRepository.SaveChangesAsync(cancellationToken);

        return new FeedbackResponse(
            feedback.Id,
            feedback.CustomerId,
            feedback.OrderId,
            feedback.ProductId,
            feedback.Rating,
            feedback.Comment,
            feedback.ImageUrl,
            feedback.Status.ToString().ToUpperInvariant(),
            feedback.ReviewedBy,
            feedback.CreatedAt,
            feedback.UpdatedAt);
    }
}
