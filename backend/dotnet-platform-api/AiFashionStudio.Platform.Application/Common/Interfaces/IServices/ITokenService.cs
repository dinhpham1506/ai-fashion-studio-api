using AiFashionStudio.Platform.Domain.Identity.Entities;

namespace AiFashionStudio.Platform.Application.Common.Interfaces.IServices;

public record AccessTokenResult(string Token, int ExpiresInSeconds);

public interface ITokenService
{
    TimeSpan RefreshTokenLifetime { get; }

    AccessTokenResult GenerateAccessToken(User user, IReadOnlyCollection<string> roleCodes);

    string GenerateRefreshToken();

    string HashRefreshToken(string refreshToken);
}
