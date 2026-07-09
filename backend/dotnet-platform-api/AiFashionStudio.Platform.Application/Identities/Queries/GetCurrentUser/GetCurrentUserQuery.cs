using AiFashionStudio.Platform.Application.Common.Dtos;
using MediatR;

namespace AiFashionStudio.Platform.Application.Identities.Queries.GetCurrentUser;

public record GetCurrentUserQuery(Guid UserId) : IRequest<CurrentUserResponse>;
