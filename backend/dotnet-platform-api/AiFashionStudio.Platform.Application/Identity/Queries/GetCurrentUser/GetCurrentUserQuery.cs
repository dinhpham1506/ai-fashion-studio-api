using AiFashionStudio.Platform.Application.Common.Dtos;
using MediatR;

namespace AiFashionStudio.Platform.Application.Identity.Queries.GetCurrentUser;

public record GetCurrentUserQuery(Guid UserId) : IRequest<CurrentUserResponse>;
