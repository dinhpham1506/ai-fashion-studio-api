using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using AiFashionStudio.Platform.Application.Common.Interfaces.IServices;
using AiFashionStudio.Platform.Domain.Identity.Entities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace AiFashionStudio.Platform.Infrastructure.Identity;


/// <summary>
/// 
/// </summary>
public class TokenService : ITokenService
{
    private readonly JwtSettings _settings;

    public TokenService(IOptions<JwtSettings> options)
    {
        _settings = options.Value;
    }

    public TimeSpan RefreshTokenLifetime => TimeSpan.FromDays(_settings.RefreshTokenExpirationDays);

    public AccessTokenResult GenerateAccessToken(User user, IReadOnlyCollection<string> roleCodes)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new("full_name", user.FullName)
        };

        claims.AddRange(roleCodes.Select(roleCode => new Claim(ClaimTypes.Role, roleCode)));

        var expiresInSeconds = _settings.AccessTokenExpirationMinutes * 60;
        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Secret));
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _settings.Issuer,
            audience: _settings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddSeconds(expiresInSeconds),
            signingCredentials: credentials);

        return new AccessTokenResult(new JwtSecurityTokenHandler().WriteToken(token), expiresInSeconds);
    }

    public string GenerateRefreshToken() => Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

    public string HashRefreshToken(string refreshToken)
    {
        var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(refreshToken));
        return Convert.ToHexString(hashBytes);
    }
}
