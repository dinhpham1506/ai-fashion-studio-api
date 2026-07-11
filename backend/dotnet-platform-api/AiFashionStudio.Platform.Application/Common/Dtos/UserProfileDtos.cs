using System;
using System.Collections.Generic;

namespace AiFashionStudio.Platform.Application.Common.Dtos;

public record UserProfileResponse(
    Guid Id,
    string Email,
    string FullName,
    string? Phone,
    string? AvatarUrl,
    IReadOnlyCollection<string> Roles,
    string Status);

public record AvatarUploadResponse(string AvatarUrl);
