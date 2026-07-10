using AiFashionStudio.Platform.Application.Feedbacks.Commands.ModerateFeedback;
using AiFashionStudio.Platform.Application.Feedbacks.Commands.SubmitFeedback;
using FluentValidation;
using Xunit;

namespace AiFashionStudio.Platform.Tests.Feedbacks;

public class FeedbackValidatorsTests
{
    [Fact]
    public void SubmitFeedbackValidator_Should_Reject_Rating_Outside_Allowed_Range()
    {
        var validator = new SubmitFeedbackCommandValidator();
        var command = new SubmitFeedbackCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            6,
            "Too high",
            null,
            null,
            null);

        var result = validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == "Rating");
    }

    [Fact]
    public void SubmitFeedbackValidator_Should_Accept_Supported_Image_Content_Type()
    {
        var validator = new SubmitFeedbackCommandValidator();
        var command = new SubmitFeedbackCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            5,
            "Great",
            [1, 2, 3],
            "image/png",
            "feedback.png");

        var result = validator.Validate(command);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void ModerateFeedbackValidator_Should_Reject_Unsupported_Status()
    {
        var validator = new ModerateFeedbackCommandValidator();
        var command = new ModerateFeedbackCommand(Guid.NewGuid(), "PENDING", Guid.NewGuid());

        var result = validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == "Status");
    }
}
