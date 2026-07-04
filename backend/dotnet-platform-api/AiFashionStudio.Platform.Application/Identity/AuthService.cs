using System.Text;
using AiFashionStudio.Platform.Domain.Identity;

namespace AiFashionStudio.Platform.Application.Identity;

/// <summary>Incoming credentials for a login attempt.</summary>
public sealed record LoginRequest(string Email, string Password);

/// <summary>Outcome of a login attempt.</summary>
public sealed record LoginResult(bool Success, string? Token);

/// <summary>Orchestrates authentication for a login request.</summary>
public interface IAuthService
{
    LoginResult Login(LoginRequest request);
}

/// <summary>
/// Validates a login request and issues a stub token. Persistence and real
/// credential checking are intentionally out of scope for this slice.
/// </summary>
public sealed class AuthService : IAuthService
{
    public LoginResult Login(LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        {
            return new LoginResult(false, null);
        }

        // Represent the authenticated principal via the domain model to show layering.
        var user = User.Create(request.Email, HashPassword(request.Password));
        var token = Convert.ToBase64String(Encoding.UTF8.GetBytes(user.Email));

        return new LoginResult(true, token);
    }

    private static string HashPassword(string password)
        => Convert.ToBase64String(Encoding.UTF8.GetBytes(password));
}
