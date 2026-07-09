using AiFashionStudio.Platform.Api.Common;
using AiFashionStudio.Platform.Application.Users.Commands.UpdateMyProfile;
using AiFashionStudio.Platform.Application.Users.Commands.UploadMyAvatar;
using AiFashionStudio.Platform.Application.Users.Queries.GetMyProfile;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AiFashionStudio.Platform.Api.Controllers;

public record UpdateProfileRequest(string FullName, string? Phone);

[ApiController]
[Route("api/users/me")]
[Authorize]
public class UserProfileController : ControllerBase
{
    private readonly ISender _sender;

    public UserProfileController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile(CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized(ApiResponse.Fail("Invalid token", Array.Empty<ApiError>()));
        }

        var result = await _sender.Send(new GetMyProfileQuery(userId), cancellationToken);
        return Ok(ApiResponse.Ok(result));
    }

    [HttpPatch("profile")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized(ApiResponse.Fail("Invalid token", Array.Empty<ApiError>()));
        }

        var result = await _sender.Send(
            new UpdateMyProfileCommand(userId, request.FullName, request.Phone), cancellationToken);
        return Ok(ApiResponse.Ok(result, "Profile updated"));
    }

    [HttpPost("avatar")]
    public async Task<IActionResult> UploadAvatar(IFormFile file, CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized(ApiResponse.Fail("Invalid token", Array.Empty<ApiError>()));
        }

        if (file is null || file.Length == 0)
        {
            return BadRequest(ApiResponse.Fail(
                "Avatar file is required",
                new[] { new ApiError("AVATAR_FILE_REQUIRED", "Avatar file is required") }));
        }

        using var memoryStream = new MemoryStream();
        await file.CopyToAsync(memoryStream, cancellationToken);

        var result = await _sender.Send(
            new UploadMyAvatarCommand(userId, memoryStream.ToArray(), file.ContentType, file.FileName),
            cancellationToken);

        return Ok(ApiResponse.Ok(result, "Avatar uploaded"));
    }

    private bool TryGetUserId(out Guid userId)
    {
        userId = Guid.Empty;
        var subject = User.Claims.FirstOrDefault(claim => claim.Type == "sub")?.Value;
        return subject is not null && Guid.TryParse(subject, out userId);
    }
}
