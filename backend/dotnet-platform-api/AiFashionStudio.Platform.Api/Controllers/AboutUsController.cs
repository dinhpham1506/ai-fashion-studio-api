using AiFashionStudio.Platform.Api.Common;
using AiFashionStudio.Platform.Application.Contents.Commands.UpsertAboutUsSection;
using AiFashionStudio.Platform.Application.Contents.Queries.GetPublishedAboutUs;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AiFashionStudio.Platform.Api.Controllers;

public record UpsertAboutUsRequest(string Title, string Content, string? ImageUrl, string Status);

[ApiController]
public class AboutUsController : ControllerBase
{
    private readonly ISender _sender;

    public AboutUsController(ISender sender)
    {
        _sender = sender;
    }

    // Public — Guest/Customer xem các section PUBLISHED
    [AllowAnonymous]
    [HttpGet("api/about-us")]
    public async Task<IActionResult> GetPublished(CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetPublishedAboutUsQuery(), cancellationToken);
        return Ok(ApiResponse.Ok(result));
    }

    // Staff/Admin — tạo mới hoặc cập nhật một section
    [Authorize(Roles = "STAFF,ADMIN")]
    [HttpPut("api/admin/about-us/{sectionKey}")]
    public async Task<IActionResult> Upsert(string sectionKey, [FromBody] UpsertAboutUsRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized(ApiResponse.Fail("Invalid token", Array.Empty<ApiError>()));
        }

        var result = await _sender.Send(
            new UpsertAboutUsSectionCommand(sectionKey, request.Title, request.Content, request.ImageUrl, request.Status, userId),
            cancellationToken);

        return Ok(ApiResponse.Ok(result, "About Us section updated"));
    }

    private bool TryGetUserId(out Guid userId)
    {
        userId = Guid.Empty;
        var subject = User.Claims.FirstOrDefault(claim => claim.Type == "sub")?.Value;
        return subject is not null && Guid.TryParse(subject, out userId);
    }
}
