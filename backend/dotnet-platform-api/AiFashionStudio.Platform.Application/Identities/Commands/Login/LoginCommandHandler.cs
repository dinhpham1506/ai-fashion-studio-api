using AiFashionStudio.Platform.Application.Common.Exceptions;
using AiFashionStudio.Platform.Application.Common.Interfaces.IServices;
using AiFashionStudio.Platform.Application.Common.Dtos;
using AiFashionStudio.Platform.Application.Common.Interfaces.IRepositories;
using AiFashionStudio.Platform.Domain.Identity.Entities;
using AiFashionStudio.Platform.Domain.Identity.Enums;
using MediatR;

namespace AiFashionStudio.Platform.Application.Identities.Commands.Login;

public class LoginCommandHandler : IRequestHandler<LoginCommand, LoginResult>
{
    private readonly IUserRepository _userRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;

    public LoginCommandHandler(
        IUserRepository userRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IPasswordHasher passwordHasher,
        ITokenService tokenService)
    {
        _userRepository = userRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
    }

    public async Task<LoginResult> Handle(LoginCommand command, CancellationToken cancellationToken)
    {
        // Lấy người dùng theo email
        var user = await _userRepository.GetByEmailAsync(command.Email, cancellationToken);

        // Kiểm tra người dùng tồn tại và mật khẩu hợp lệ
        if (user is null || !_passwordHasher.Verify(command.Password, user.PasswordHash))
        {
            // Nếu người dùng không tồn tại hoặc mật khẩu không hợp lệ, ném ra ngoại lệ UnauthorizedException
            throw new UnauthorizedException("INVALID_CREDENTIALS", "Invalid email or password");
        }

        if (user.Status == UserStatus.Banned)
        {
            // Nếu người dùng bị cấm, ném ra ngoại lệ ForbiddenException
            throw new ForbiddenException("USER_BANNED", "User is banned");
        }

        if (user.Status == UserStatus.Inactive)
        {
            // Nếu người dùng không hoạt động, ném ra ngoại lệ ForbiddenException
            throw new ForbiddenException("USER_INACTIVE", "User is inactive");
        }
        
        // Lấy danh sách mã vai trò của người dùng
        var roleCodes = user.UserRoles.Select(userRole => userRole.Role.Code.ToString().ToUpperInvariant()).ToList();

        // Tạo access token và refresh token
        var accessToken = _tokenService.GenerateAccessToken(user, roleCodes);
        var rawRefreshToken = _tokenService.GenerateRefreshToken();

        // Hash refresh token và lưu vào cơ sở dữ liệu
        var tokenHash = _tokenService.HashRefreshToken(rawRefreshToken);
        var expiresAt = DateTime.UtcNow.Add(_tokenService.RefreshTokenLifetime);

        var refreshToken = RefreshToken.Create(user.Id, tokenHash, expiresAt);
        await _refreshTokenRepository.AddAsync(refreshToken, cancellationToken);

        // Tạo response chứa access token, refresh token và thông tin người dùng
        var response = new LoginResponse(
            accessToken.Token,
            accessToken.ExpiresInSeconds,
            new UserSummaryResponse(user.Id, user.Email, user.FullName, roleCodes));

        return new LoginResult(response, rawRefreshToken, expiresAt);
    }
}
