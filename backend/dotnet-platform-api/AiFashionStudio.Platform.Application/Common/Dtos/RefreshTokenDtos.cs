namespace AiFashionStudio.Platform.Application.Common.Dtos;

// Kết quả trả về cho controller: chỉ kèm access token
public record RefreshTokenResponse(string AccessToken, int ExpiresIn);

// Kết quả trả về cho controller: kèm access token và refresh token
public record RefreshTokenResult(RefreshTokenResponse Response, string RefreshToken, DateTime RefreshTokenExpiresAt);
