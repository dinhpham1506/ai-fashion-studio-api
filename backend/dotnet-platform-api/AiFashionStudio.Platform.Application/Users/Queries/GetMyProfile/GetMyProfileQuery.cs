using AiFashionStudio.Platform.Application.Common.Dtos;
using MediatR;
using System;

namespace AiFashionStudio.Platform.Application.Users.Queries.GetMyProfile
{
    public record GetMyProfileQuery(Guid UserId) : IRequest<UserProfileResponse>;
}
