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

    /// <summary>
    /// Creates an About Us controller with the specified sender.
    /// </summary>
    /// <param name="sender">The mediator used to execute queries and commands.</param>
    public AboutUsController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// Gets the published About Us content.
    /// </summary>
    /// <returns>The published About Us content wrapped in an OK response.</returns>
    [AllowAnonymous]
    [HttpGet("api/about-us")]
    public async Task<IActionResult> GetPublished(CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetPublishedAboutUsQuery(), cancellationToken);
        return Ok(ApiResponse.Ok(result));
    }

    /// <summary>
    /// Creates or updates an About Us section.
    /// </summary>
    /// <param name="sectionKey">The section identifier to upsert.</param>
    /// <param name="request">The About Us content to store.</param>
    /// <returns>An action result containing the updated section, or an unauthorized response when the user token is invalid.</returns>
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

    /// <summary>
    /// Extracts the current user's identifier from the subject claim.
    /// </summary>
    /// <param name="userId">When this method returns, contains the parsed user identifier if the claim is present and valid; otherwise, <see cref="Guid.Empty"/>.</param>
    /// <returns><c>true</c> if the subject claim exists and can be parsed as a <see cref="Guid"/>, <c>false</c> otherwise.</returns>
    private bool TryGetUserId(out Guid userId)
    {
        userId = Guid.Empty;
        var subject = User.Claims.FirstOrDefault(claim => claim.Type == "sub")?.Value;
        return subject is not null && Guid.TryParse(subject, out userId);
    }
}
