namespace AiFashionStudio.Platform.Application.Common.Dtos;

// trả về thông tin người dùng hiện tại
public record CurrentUserResponse(
    Guid Id,
    string Email,
    string FullName,
    string? Phone,
    string? AvatarUrl,
    IReadOnlyCollection<string> Roles,
    string Status);
