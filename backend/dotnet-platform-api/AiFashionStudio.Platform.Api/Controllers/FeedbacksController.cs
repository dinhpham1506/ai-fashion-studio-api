using AiFashionStudio.Platform.Api.Common;
using AiFashionStudio.Platform.Application.Feedbacks.Commands.ModerateFeedback;
using AiFashionStudio.Platform.Application.Feedbacks.Commands.SubmitFeedback;
using AiFashionStudio.Platform.Application.Feedbacks.Queries.GetFeedbacksForModeration;
using AiFashionStudio.Platform.Application.Feedbacks.Queries.GetPublicFeedbacks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AiFashionStudio.Platform.Api.Controllers;

public record ModerateFeedbackRequest(string Status);

public class SubmitFeedbackRequest
{
    public Guid OrderId { get; init; }
    public Guid ProductId { get; init; }
    public int Rating { get; init; }
    public string? Comment { get; init; }
    public IFormFile? Image { get; init; }
}

[ApiController]
public class FeedbacksController : ControllerBase
{
    private readonly ISender _sender;

    public FeedbacksController(ISender sender)
    {
        _sender = sender;
    }

    [Authorize]
    [HttpPost("api/feedbacks")]
    [RequestFormLimits(MultipartBodyLengthLimit = 5 * 1024 * 1024)]
    public async Task<IActionResult> Submit([FromForm] SubmitFeedbackRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized(ApiResponse.Fail("Invalid token", Array.Empty<ApiError>()));
        }

        byte[]? imageContent = null;
        if (request.Image is not null)
        {
            await using var memoryStream = new MemoryStream();
            await request.Image.CopyToAsync(memoryStream, cancellationToken);
            imageContent = memoryStream.ToArray();
        }

        var result = await _sender.Send(
            new SubmitFeedbackCommand(
                userId,
                request.OrderId,
                request.ProductId,
                request.Rating,
                request.Comment,
                imageContent,
                request.Image?.ContentType,
                request.Image?.FileName),
            cancellationToken);

        return Ok(ApiResponse.Ok(result, "Feedback submitted and is pending moderation."));
    }

    [AllowAnonymous]
    [HttpGet("api/feedbacks/public")]
    public async Task<IActionResult> GetPublic([FromQuery] Guid? productId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10, CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(new GetPublicFeedbacksQuery(productId, page, pageSize), cancellationToken);
        return Ok(ApiResponse.Ok(result));
    }

    [Authorize(Roles = "STAFF,ADMIN")]
    [HttpGet("api/admin/feedbacks")]
    public async Task<IActionResult> GetForModeration([FromQuery] string? status, [FromQuery] Guid? productId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(
            new GetFeedbacksForModerationQuery(status, productId, page, pageSize),
            cancellationToken);

        return Ok(ApiResponse.Ok(result));
    }

    [Authorize(Roles = "STAFF,ADMIN")]
    [HttpPatch("api/admin/feedbacks/{feedbackId:guid}/moderation")]
    public async Task<IActionResult> Moderate(Guid feedbackId, [FromBody] ModerateFeedbackRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized(ApiResponse.Fail("Invalid token", Array.Empty<ApiError>()));
        }

        var result = await _sender.Send(
            new ModerateFeedbackCommand(feedbackId, request.Status, userId),
            cancellationToken);

        return Ok(ApiResponse.Ok(result, "Feedback moderation updated."));
    }

    private bool TryGetUserId(out Guid userId)
    {
        userId = Guid.Empty;
        var subject = User.Claims.FirstOrDefault(claim => claim.Type == "sub")?.Value;
        return subject is not null && Guid.TryParse(subject, out userId);
    }
}
