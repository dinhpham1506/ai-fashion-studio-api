using AiFashionStudio.Platform.Api.Common;
using AiFashionStudio.Platform.Application.Identity.Commands.ForgotPassword;
using AiFashionStudio.Platform.Application.Identity.Commands.Login;
using AiFashionStudio.Platform.Application.Identity.Commands.Logout;
using AiFashionStudio.Platform.Application.Identity.Commands.RefreshAccessToken;
using AiFashionStudio.Platform.Application.Identity.Commands.Register;
using AiFashionStudio.Platform.Application.Identity.Commands.ResetPassword;
using AiFashionStudio.Platform.Application.Identity.Commands.VerifyResetOtp;
using AiFashionStudio.Platform.Application.Identity.Queries.GetCurrentUser;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AiFashionStudio.Platform.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private const string RefreshTokenCookieName = "refresh_token";

    private readonly ISender _sender;

    public AuthController(ISender sender)
    {
        _sender = sender;
    }

    // Refresh token chỉ đi qua HttpOnly cookie để JavaScript phía client không đọc được (chống XSS).
    // Path giới hạn cookie chỉ gửi kèm các request tới /api/auth.
    private void SetRefreshTokenCookie(string refreshToken, DateTime expiresAt)
    {
        Response.Cookies.Append(RefreshTokenCookieName, refreshToken, BuildRefreshTokenCookieOptions(expiresAt));
    }

    private void DeleteRefreshTokenCookie()
    {
        Response.Cookies.Delete(RefreshTokenCookieName, BuildRefreshTokenCookieOptions(expiresAt: null));
    }

    private static CookieOptions BuildRefreshTokenCookieOptions(DateTime? expiresAt) => new()
    {
        HttpOnly = true,
        Secure = true,
        SameSite = SameSiteMode.None,
        Path = "/api/auth",
        Expires = expiresAt
    };

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterCommand command, CancellationToken cancellationToken)
    {
        await _sender.Send(command, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, ApiResponse.Ok("Register successfully"));
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginCommand command, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(command, cancellationToken);
        SetRefreshTokenCookie(result.RefreshToken, result.RefreshTokenExpiresAt);
        return Ok(ApiResponse.Ok(result.Response, "Login successfully"));
    }

    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken(CancellationToken cancellationToken)
    {
        var refreshToken = Request.Cookies[RefreshTokenCookieName];

        if (string.IsNullOrEmpty(refreshToken))
        {
            return Unauthorized(ApiResponse.Fail(
                "Refresh token is missing",
                new[] { new ApiError("REFRESH_TOKEN_REQUIRED", "Refresh token is missing") }));
        }

        var result = await _sender.Send(new RefreshAccessTokenCommand(refreshToken), cancellationToken);
        SetRefreshTokenCookie(result.RefreshToken, result.RefreshTokenExpiresAt);
        return Ok(ApiResponse.Ok(result.Response, "Token refreshed"));
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout(CancellationToken cancellationToken)
    {
        var refreshToken = Request.Cookies[RefreshTokenCookieName];

        if (!string.IsNullOrEmpty(refreshToken))
        {
            await _sender.Send(new LogoutCommand(refreshToken), cancellationToken);
        }

        DeleteRefreshTokenCookie();
        return Ok(ApiResponse.Ok("Logout successfully"));
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> Me(CancellationToken cancellationToken)
    {
        var subject = User.Claims.FirstOrDefault(claim => claim.Type == "sub")?.Value;

        if (subject is null || !Guid.TryParse(subject, out var userId))
        {
            return Unauthorized(ApiResponse.Fail("Invalid token", Array.Empty<ApiError>()));
        }

        var result = await _sender.Send(new GetCurrentUserQuery(userId), cancellationToken);
        return Ok(ApiResponse.Ok(result));
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordCommand command, CancellationToken cancellationToken)
    {
        await _sender.Send(command, cancellationToken);
        return Ok(ApiResponse.Ok("If the email exists, an OTP has been sent"));
    }

    [HttpPost("verify-reset-otp")]
    public async Task<IActionResult> VerifyResetOtp([FromBody] VerifyResetOtpCommand command, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(command, cancellationToken);
        return Ok(ApiResponse.Ok(result, "OTP verified"));
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordCommand command, CancellationToken cancellationToken)
    {
        await _sender.Send(command, cancellationToken);
        return Ok(ApiResponse.Ok("Password has been reset"));
    }
}
