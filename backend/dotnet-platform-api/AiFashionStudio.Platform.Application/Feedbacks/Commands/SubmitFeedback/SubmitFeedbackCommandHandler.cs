using AiFashionStudio.Platform.Application.Common.Dtos;
using AiFashionStudio.Platform.Application.Common.Exceptions;
using AiFashionStudio.Platform.Application.Common.Interfaces.IRepositories;
using AiFashionStudio.Platform.Application.Common.Interfaces.IServices;
using AiFashionStudio.Platform.Domain.Feedback.Entities;
using MediatR;

namespace AiFashionStudio.Platform.Application.Feedbacks.Commands.SubmitFeedback;

public class SubmitFeedbackCommandHandler : IRequestHandler<SubmitFeedbackCommand, FeedbackResponse>
{
    private const string FeedbackBucket = "feedbacks";

    private readonly IFeedbackRepository _feedbackRepository;
    private readonly IFileStorage _fileStorage;

    public SubmitFeedbackCommandHandler(IFeedbackRepository feedbackRepository, IFileStorage fileStorage)
    {
        _feedbackRepository = feedbackRepository;
        _fileStorage = fileStorage;
    }

    public async Task<FeedbackResponse> Handle(SubmitFeedbackCommand command, CancellationToken cancellationToken)
    {
        var eligibility = await _feedbackRepository.GetOrderEligibilityAsync(
            command.CustomerId,
            command.OrderId,
            command.ProductId,
            cancellationToken);

        if (eligibility is null || !eligibility.OrderExists)
        {
            throw new NotFoundException("ORDER_NOT_FOUND", "Order not found.");
        }

        if (!eligibility.IsCompleted)
        {
            throw new ConflictException("ORDER_NOT_COMPLETED", "Feedback is only allowed when the order is completed.");
        }

        if (!eligibility.ProductBelongsToOrder)
        {
            throw new AppValidationException("productId", "PRODUCT_NOT_IN_ORDER", "The selected product does not belong to this order.");
        }

        var alreadySubmitted = await _feedbackRepository.ExistsByCustomerOrderProductAsync(
            command.CustomerId,
            command.OrderId,
            command.ProductId,
            cancellationToken);

        if (alreadySubmitted)
        {
            throw new ConflictException("FEEDBACK_ALREADY_EXISTS", "Feedback for this product and order already exists.");
        }

        string? imageUrl = null;
        if (command.ImageContent is not null && command.ImageContentType is not null && command.ImageFileName is not null)
        {
            var extension = Path.GetExtension(command.ImageFileName);
            var objectName = $"{command.CustomerId}/{DateTime.UtcNow:yyyy/MM}/{Guid.NewGuid():N}{extension}";
            imageUrl = await _fileStorage.UploadAsync(
                FeedbackBucket,
                objectName,
                command.ImageContent,
                command.ImageContentType,
                cancellationToken);
        }

        var feedback = Feedback.Create(
            command.CustomerId,
            command.OrderId,
            command.ProductId,
            command.Rating,
            command.Comment?.Trim(),
            imageUrl);

        await _feedbackRepository.AddAsync(feedback, cancellationToken);

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
