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

        public GetMyProfileQueryHandler(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<UserProfileResponse> Handle(GetMyProfileQuery request, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetByIdWithRolesAsync(request.UserId, cancellationToken)
                ?? throw new UnauthorizedException("INVALID_CREDENTIALS", "User not found");

            return MapToResponse(user);
        }

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
