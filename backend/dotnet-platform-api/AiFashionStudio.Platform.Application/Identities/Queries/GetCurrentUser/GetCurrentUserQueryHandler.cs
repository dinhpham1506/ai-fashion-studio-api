using AiFashionStudio.Platform.Application.Common.Exceptions;
using AiFashionStudio.Platform.Application.Common.Dtos;
using AiFashionStudio.Platform.Application.Common.Interfaces.IRepositories;
using MediatR;

namespace AiFashionStudio.Platform.Application.Identities.Queries.GetCurrentUser;

public class GetCurrentUserQueryHandler : IRequestHandler<GetCurrentUserQuery, CurrentUserResponse>
{
    private readonly IUserRepository _userRepository;

    public GetCurrentUserQueryHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<CurrentUserResponse> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdWithRolesAsync(request.UserId, cancellationToken)
            ?? throw new UnauthorizedException("INVALID_CREDENTIALS", "User not found");

        var roleCodes = user.UserRoles.Select(userRole => userRole.Role.Code.ToString().ToUpperInvariant()).ToList();

        return new CurrentUserResponse(
            user.Id,
            user.Email,
            user.FullName,
            user.Phone,
            user.AvatarUrl,
            roleCodes,
            user.Status.ToString().ToUpperInvariant());
    }
}
