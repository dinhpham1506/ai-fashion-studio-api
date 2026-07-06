using AiFashionStudio.Platform.Application.Common.Exceptions;
using AiFashionStudio.Platform.Application.Common.Dtos;
using AiFashionStudio.Platform.Application.Common.Interfaces.IRepositories;
using AiFashionStudio.Platform.Application.Common.Interfaces.IServices;
using AiFashionStudio.Platform.Domain.Identity.Entities;
using MediatR;

namespace AiFashionStudio.Platform.Application.Identity.Commands.RefreshAccessToken;

public class RefreshAccessTokenCommandHandler : IRequestHandler<RefreshAccessTokenCommand, RefreshTokenResult>
{
    private readonly IUserRepository _userRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly ITokenService _tokenService;

    public RefreshAccessTokenCommandHandler(
        IUserRepository userRepository,
        IRefreshTokenRepository refreshTokenRepository,
        ITokenService tokenService)
    {
        _userRepository = userRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _tokenService = tokenService;
    }

    public async Task<RefreshTokenResult> Handle(RefreshAccessTokenCommand command, CancellationToken cancellationToken)
    {
        // Kiểm tra refresh token có hợp lệ không
        var tokenHash = _tokenService.HashRefreshToken(command.RefreshToken);
        // Lấy refresh token từ cơ sở dữ liệu theo hash
        var existingToken = await _refreshTokenRepository.GetByTokenHashAsync(tokenHash, cancellationToken);

        // Kiểm tra xem refresh token có tồn tại và chưa bị thu hồi không
        if (existingToken is null || existingToken.RevokedAt is not null)
        {
            throw new UnauthorizedException("REFRESH_TOKEN_REVOKED", "Refresh token has been revoked");
        }

        if (!existingToken.IsActive())
        {
            throw new UnauthorizedException("REFRESH_TOKEN_EXPIRED", "Refresh token has expired");
        }

        // Lấy người dùng theo UserId từ refresh token
        var user = await _userRepository.GetByIdWithRolesAsync(existingToken.UserId, cancellationToken)
            ?? throw new UnauthorizedException("INVALID_CREDENTIALS", "User not found");

        // Thu hồi refresh token hiện tại
        existingToken.Revoke();
        await _refreshTokenRepository.UpdateAsync(existingToken, cancellationToken);

        // Tạo access token và refresh token mới
        var roleCodes = user.UserRoles.Select(userRole => userRole.Role.Code.ToString().ToUpperInvariant()).ToList();
        var accessToken = _tokenService.GenerateAccessToken(user, roleCodes);
        var rawRefreshToken = _tokenService.GenerateRefreshToken();
        var newTokenHash = _tokenService.HashRefreshToken(rawRefreshToken);
        var expiresAt = DateTime.UtcNow.Add(_tokenService.RefreshTokenLifetime);

        var newRefreshToken = RefreshToken.Create(user.Id, newTokenHash, expiresAt);
        await _refreshTokenRepository.AddAsync(newRefreshToken, cancellationToken);

        return new RefreshTokenResult(
            new RefreshTokenResponse(accessToken.Token, accessToken.ExpiresInSeconds),
            rawRefreshToken,
            expiresAt);
    }
}
