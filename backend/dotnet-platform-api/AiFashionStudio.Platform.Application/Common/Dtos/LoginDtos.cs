namespace AiFashionStudio.Platform.Application.Common.Dtos;

// tổng quan thông tin người dùng
public record UserSummaryResponse(Guid Id, string Email, string FullName, IReadOnlyCollection<string> Roles);

// trả về thông tin đăng nhập
public record LoginResponse(string AccessToken, int ExpiresIn, UserSummaryResponse User);

// trả về thông tin đăng nhập kèm refresh token
public record LoginResult(LoginResponse Response, string RefreshToken, DateTime RefreshTokenExpiresAt);
