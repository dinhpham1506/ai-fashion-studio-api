using AiFashionStudio.Platform.Application.Common.Dtos;
using MediatR;

namespace AiFashionStudio.Platform.Application.Feedbacks.Commands.ModerateFeedback;

public record ModerateFeedbackCommand(Guid FeedbackId, string Status, Guid ReviewedBy) : IRequest<FeedbackResponse>;
