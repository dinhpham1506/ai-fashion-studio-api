using AiFashionStudio.Platform.Application.Common.Dtos;
using MediatR;
using System;

namespace AiFashionStudio.Platform.Application.Users.Commands.UpdateMyProfile
{
    public record UpdateMyProfileCommand(Guid UserId, string FullName, string? Phone) : IRequest<UserProfileResponse>;
}
