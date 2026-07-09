using AiFashionStudio.Platform.Application.Common.Dtos;
using MediatR;
using System;

namespace AiFashionStudio.Platform.Application.Users.Commands.UploadMyAvatar
{
    public record UploadMyAvatarCommand(Guid UserId, byte[] Content, string ContentType, string FileName) : IRequest<AvatarUploadResponse>;
}
