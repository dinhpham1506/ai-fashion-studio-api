using AiFashionStudio.Platform.Application.Common.Dtos;
using AiFashionStudio.Platform.Application.Common.Exceptions;
using AiFashionStudio.Platform.Application.Common.Interfaces.IRepositories;
using AiFashionStudio.Platform.Application.Users.Queries.GetMyProfile;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace AiFashionStudio.Platform.Application.Users.Commands.UpdateMyProfile
{
    public class UpdateMyProfileCommandHandler : IRequestHandler<UpdateMyProfileCommand, UserProfileResponse>
    {
        private readonly IUserRepository _userRepository;

        public UpdateMyProfileCommandHandler(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<UserProfileResponse> Handle(UpdateMyProfileCommand command, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetByIdWithRolesAsync(command.UserId, cancellationToken)
                ?? throw new UnauthorizedException("INVALID_CREDENTIALS", "User not found");

            user.UpdateProfile(command.FullName.Trim(), string.IsNullOrWhiteSpace(command.Phone) ? null : command.Phone.Trim());
            await _userRepository.SaveChangesAsync(cancellationToken);

            return GetMyProfileQueryHandler.MapToResponse(user);
        }
    }
}
