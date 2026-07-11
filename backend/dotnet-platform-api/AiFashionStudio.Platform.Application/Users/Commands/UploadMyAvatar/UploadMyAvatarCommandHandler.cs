using AiFashionStudio.Platform.Application.Common.Dtos;
using AiFashionStudio.Platform.Application.Common.Exceptions;
using AiFashionStudio.Platform.Application.Common.Interfaces.IRepositories;
using AiFashionStudio.Platform.Application.Common.Interfaces.IServices;
using MediatR;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace AiFashionStudio.Platform.Application.Users.Commands.UploadMyAvatar
{
    public class UploadMyAvatarCommandHandler : IRequestHandler<UploadMyAvatarCommand, AvatarUploadResponse>
    {
        private const string AvatarBucket = "avatars";

        private readonly IUserRepository _userRepository;
        private readonly IFileStorage _fileStorage;

        /// <summary>
        /// Initializes a new instance of the <see cref="UploadMyAvatarCommandHandler"/> class.
        /// </summary>
        public UploadMyAvatarCommandHandler(IUserRepository userRepository, IFileStorage fileStorage)
        {
            _userRepository = userRepository;
            _fileStorage = fileStorage;
        }

        /// <summary>
        /// Uploads the authenticated user's avatar and updates the stored profile image URL.
        /// </summary>
        /// <param name="command">The upload command containing the user ID, file name, content, and content type.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>The uploaded avatar URL.</returns>
        public async Task<AvatarUploadResponse> Handle(UploadMyAvatarCommand command, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetByIdAsync(command.UserId, cancellationToken)
                ?? throw new UnauthorizedException("INVALID_CREDENTIALS", "User not found");

            // Tên object unique theo user để avatar mới không đè lịch sử file cũ ngoài ý muốn
            var extension = Path.GetExtension(command.FileName);
            var objectName = $"{command.UserId}/{Guid.NewGuid():N}{extension}";

            var avatarUrl = await _fileStorage.UploadAsync(
                AvatarBucket, objectName, command.Content, command.ContentType, cancellationToken);

            user.ChangeAvatar(avatarUrl);
            await _userRepository.SaveChangesAsync(cancellationToken);

            return new AvatarUploadResponse(avatarUrl);
        }
    }
}
