using AiFashionStudio.Platform.Application.Common.Dtos;
using AiFashionStudio.Platform.Application.Common.Exceptions;
using AiFashionStudio.Platform.Application.Common.Interfaces.IRepositories;
using AiFashionStudio.Platform.Domain.Identity.Entities;
using MediatR;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AiFashionStudio.Platform.Application.Users.Queries.GetMyProfile
{
    public class GetMyProfileQueryHandler : IRequestHandler<GetMyProfileQuery, UserProfileResponse>
    {
        private readonly IUserRepository _userRepository;

        /// <summary>
        /// Creates a new instance of the <see cref="GetMyProfileQueryHandler"/> class.
        /// </summary>
        /// <param name="userRepository">The user repository used to load the current user's profile.</param>
        public GetMyProfileQueryHandler(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        /// <summary>
        /// Gets the current user's profile.
        /// </summary>
        /// <param name="request">The query containing the user identifier.</param>
        /// <param name="cancellationToken">A token that can cancel the operation.</param>
        /// <returns>The current user's profile.</returns>
        /// <exception cref="UnauthorizedException">Thrown when the user cannot be found.</exception>
        public async Task<UserProfileResponse> Handle(GetMyProfileQuery request, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetByIdWithRolesAsync(request.UserId, cancellationToken)
                ?? throw new UnauthorizedException("INVALID_CREDENTIALS", "User not found");

            return MapToResponse(user);
        }

        /// <summary>
        /// Creates a user profile response from a user entity.
        /// </summary>
        /// <param name="user">The user to map.</param>
        /// <returns>A user profile response populated with the user's details, role codes, and status.</returns>
        internal static UserProfileResponse MapToResponse(User user)
        {
            var roleCodes = user.UserRoles
                .Select(userRole => userRole.Role.Code.ToString().ToUpperInvariant())
                .ToList();

            return new UserProfileResponse(
                user.Id,
                user.Email,
                user.FullName,
                user.Phone,
                user.AvatarUrl,
                roleCodes,
                user.Status.ToString().ToUpperInvariant());
        }
    }
}
