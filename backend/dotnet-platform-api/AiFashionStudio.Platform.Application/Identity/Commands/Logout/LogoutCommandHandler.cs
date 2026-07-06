using AiFashionStudio.Platform.Application.Common.Interfaces.IRepositories;
using AiFashionStudio.Platform.Application.Common.Interfaces.IServices;
using MediatR;

namespace AiFashionStudio.Platform.Application.Identity.Commands.Logout;

public class LogoutCommandHandler : IRequestHandler<LogoutCommand>
{
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly ITokenService _tokenService;

    public LogoutCommandHandler(IRefreshTokenRepository refreshTokenRepository, ITokenService tokenService)
    {
        _refreshTokenRepository = refreshTokenRepository;
        _tokenService = tokenService;
    }

    public async Task Handle(LogoutCommand command, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(command.RefreshToken))
        {
            return;
        }
        // băm refresh token để tìm trong DB
        var tokenHash = _tokenService.HashRefreshToken(command.RefreshToken);
        // tìm refresh token trong DB
        var existingToken = await _refreshTokenRepository.GetByTokenHashAsync(tokenHash, cancellationToken);

        // nếu không tìm thấy hoặc đã bị thu hồi thì không làm gì cả
        if (existingToken is null || existingToken.RevokedAt is not null)
        {
            return;
        }
        // thu hồi refresh token
        existingToken.Revoke();
        // cập nhật refresh token trong DB
        await _refreshTokenRepository.UpdateAsync(existingToken, cancellationToken);
    }
}
