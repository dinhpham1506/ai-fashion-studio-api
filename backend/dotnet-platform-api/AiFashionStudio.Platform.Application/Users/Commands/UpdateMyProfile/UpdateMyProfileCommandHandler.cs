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

        /// <summary>
        /// Initializes a new instance of the <see cref="UpdateMyProfileCommandHandler"/> class.
        /// </summary>
        /// <param name="userRepository">The user repository used to load and save user data.</param>
        public UpdateMyProfileCommandHandler(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        /// <summary>
        /// Updates the authenticated user's profile.
        /// </summary>
        /// <param name="command">The profile update data, including the user ID, full name, and phone number.</param>
        /// <returns>The updated user profile.</returns>
        /// <exception cref="UnauthorizedException">Thrown when the user cannot be found.</exception>
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
